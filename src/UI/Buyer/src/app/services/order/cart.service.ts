import { Injectable } from '@angular/core'
import { Orders, LineItems, Me, LineItemSpec } from 'ordercloud-javascript-sdk'
import { Subject } from 'rxjs'
import { OrderStateService } from './order-state.service'
import { isUndefined as _isUndefined } from 'lodash'
import {
  HSLineItem,
  HSOrder,
  HeadStartSDK,
  ListPage,
} from '@ordercloud/headstart-sdk'
import { CheckoutService } from './checkout.service'
import { listAll } from '../listAll'

@Injectable({
  providedIn: 'root',
})
export class CartService {
  public onAdd = new Subject<HSLineItem>() // need to make available as observable
  public onChange: (
    callback: (lineItems: ListPage<HSLineItem>) => void
  ) => void
  private initializingOrder = false

  constructor(
    private state: OrderStateService,
    private checkout: CheckoutService
  ) {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
    this.onChange = this.state.onLineItemsChange.bind(this.state)
  }

  get(): ListPage<HSLineItem> {
    return this.lineItems
  }

  async getInvalidLineItems(): Promise<HSLineItem[]> {
    const unavailableLineItems = await this.getInactiveProducts(this.lineItems)
    if (unavailableLineItems.length) {
      return unavailableLineItems
    }
    return null
  }

  async getInactiveProducts(
    items: ListPage<HSLineItem>
  ): Promise<HSLineItem[]> {
    const inactiveLineItems: HSLineItem[] = []
    for (const item of items.Items) {
      const eligibleProductList = await Me.ListProducts({
        filters: { ID: item.ProductID },
      })
      const matchingLineItem = eligibleProductList.Items.find(
        (product) => product.ID === item.ProductID
      )
      if (!matchingLineItem) {
        inactiveLineItems.push(item)
      }
    }
    return inactiveLineItems
  }

  // TODO - get rid of the progress spinner for all Cart functions. Just makes it look slower.
  async add(lineItem: HSLineItem): Promise<HSLineItem> {
    // order is well defined, line item can be added
    this.onAdd.next(lineItem)
    if (!_isUndefined(this.order.DateCreated)) {
      const isPrintProduct = lineItem.xp.PrintArtworkURL
      // Handle quantity changes for non-print products
      if (!isPrintProduct) {
        const lineItems = this.state.lineItems.Items
        if (lineItem?.xp?.KitProductID) {
          // Kit product line item quantity changes
          const kitLiWithSameProduct = lineItems.find(
            (li) =>
              li.ProductID === lineItem.ProductID &&
              li?.xp?.KitProductID === lineItem?.xp?.KitProductID
          )
          if (
            kitLiWithSameProduct &&
            this.hasSameSpecs(lineItem, kitLiWithSameProduct)
          ) {
            // combine any line items that have the same productID/specs into one line item
            lineItem.Quantity += kitLiWithSameProduct.Quantity
          }
        } else {
          // Non-kit product line item quantity changes
          const lineItemWithMatchingSpecs = lineItems.find(
            (li) =>
              li.ProductID === lineItem.ProductID &&
              this.hasSameSpecs(lineItem, li)
          )
          if (lineItemWithMatchingSpecs) {
            lineItem.Quantity += lineItemWithMatchingSpecs.Quantity
          }
        }
      }
      return await this.upsertLineItem(lineItem)
    }
    if (!this.initializingOrder) {
      this.initializingOrder = true
      await this.state.reset()
      this.initializingOrder = false
      return await this.upsertLineItem(lineItem)
    }
  }

  async remove(lineItemID: string): Promise<void> {
    this.lineItems.Items = this.lineItems.Items.filter(
      (li) => li.ID !== lineItemID
    )
    Object.assign(this.state.order, this.calculateOrder())
    try {
      await HeadStartSDK.Orders.DeleteLineItem(this.state.order.ID, lineItemID)
    } finally {
      await this.state.reset()
    }
  }

  async removeMany(lineItems: HSLineItem[]): Promise<void[]> {
    const req = lineItems.map((li) => this.remove(li.ID))
    return Promise.all(req)
  }

  async setQuantity(
    lineItem: HSLineItem
  ): Promise<HSLineItem> {
    try {
      return await this.upsertLineItem(lineItem)
    } finally {
      await this.state.reset()
    }
  }

  async addSupplierComments(
    lineItemID: string,
    comments: string
  ): Promise<HSLineItem> {
    try {
      const lineToUpdate = this.state.lineItems.Items.find(
        (li) => li.ID === lineItemID
      )
      lineToUpdate.xp.SupplierComments = comments

      // only include properties seller can edit (exclude private addresses)
      const { ProductID, Specs, Quantity, xp } = lineToUpdate
      return await this.upsertLineItem({
        ProductID,
        Specs,
        Quantity,
        xp,
      })
    } finally {
      await this.state.reset()
    }
  }

  async addMany(
    lineItem: HSLineItem[]
  ): Promise<HSLineItem[]> {
    const req = lineItem.map((li) => this.add(li))
    return Promise.all(req)
  }

  async moveOrderToCart(orderID: string): Promise<void> {
    /* this process is to move a order into the cart which was previously marked for
     * changes by an approver. We are making the xp as IsResubmitting, then resetting the cart
     * however so that the normal unsubmitted orders (orders which were not previously declined)
     * do not repopulate in the cart after the resubmit we are deleting all of these
     * unsubmitted orders */

    const orderToUpdate = (await Orders.Patch('Outgoing', orderID, {
      xp: { IsResubmitting: true },
    })) as HSOrder

    const currentUnsubmittedOrders = await Me.ListOrders({
      sortBy: '!DateCreated',
      filters: {
        DateDeclined: '!*',
        status: 'Unsubmitted',
        'xp.OrderType': 'Standard',
      },
    })

    const deleteOrderRequests = currentUnsubmittedOrders.Items.map((c) =>
      Orders.Delete('Outgoing', c.ID)
    )
    await Promise.all(deleteOrderRequests)

    // cannot use this.state.reset because the order index isn't ready immediately after the patch of IsResubmitting
    this.state.order = orderToUpdate
    this.state.lineItems = await listAll(
      LineItems,
      LineItems.List,
      'Outgoing',
      this.order.ID
    )
  }

  async empty(): Promise<void> {
    try {
      // don't delete order, we need to keep so we can preserve
      // stuff like payment transaction history, instead delete line items on order
      const requests = this.lineItems.Items.map((li) =>
        LineItems.Delete('Outgoing', this.order.ID, li.ID)
      )
      await Promise.all(requests)
    } finally {
      await this.state.reset()
    }
  }

  private hasSameSpecs(
    line1: HSLineItem,
    line2: HSLineItem
  ): boolean {
    if (!line1?.Specs?.length && !line2?.Specs?.length) {
      return true
    }
    if (
      (!line1?.Specs?.length && line2?.Specs?.length) ||
      (line1?.Specs?.length && !line2?.Specs?.length)
    ) {
      return false
    }
    const sortedSpecs1 = line1.Specs.sort(this.sortSpecs).map((s) => ({
      SpecID: s.SpecID,
      OptionID: s.OptionID,
    }))
    const sortedSpecs2 = line2.Specs.sort(this.sortSpecs).map((s) => ({
      SpecID: s.SpecID,
      OptionID: s.OptionID,
    }))
    return JSON.stringify(sortedSpecs1) === JSON.stringify(sortedSpecs2)
  }

  private sortSpecs(a: LineItemSpec, b: LineItemSpec): number {
    // sort by SpecID, if SpecID is the same, then sort by OptionID
    if (a.SpecID === b.SpecID) {
      return a.OptionID < b.OptionID ? -1 : a.OptionID > b.OptionID ? 1 : 0
    } else {
      return a.SpecID < b.SpecID ? -1 : 1
    }
  }

  private async upsertLineItem(
    lineItem: HSLineItem
  ): Promise<HSLineItem> {
    try {
      return await HeadStartSDK.Orders.UpsertLineItem(this.order?.ID, lineItem)
    } finally {
      if (this.state.orderPromos?.Items?.length) {
        // if there are pre-existing promos need to recalculate order
        await this.checkout.calculateOrder()
      }
      await this.state.reset()
    }
  }

  private calculateOrder(): HSOrder {
    const LineItemCount = this.lineItems.Items.length
    this.lineItems.Items.forEach((li: any) => {
      li.LineTotal = li.Quantity * li.UnitPrice
      if (isNaN(li.LineTotal)) li.LineTotal = undefined
    })
    const Subtotal = this.lineItems.Items.reduce(
      (sum, li) => sum + li.LineTotal,
      0
    )
    const Total = Subtotal + this.order.TaxCost + this.order.ShippingCost
    return { LineItemCount, Total, Subtotal }
  }

  private get order(): HSOrder {
    return this.state.order
  }

  private set order(value: HSOrder) {
    this.state.order = value
  }

  private get lineItems(): ListPage<HSLineItem> {
    return this.state.lineItems
  }

  private set lineItems(value: ListPage<HSLineItem>) {
    this.state.lineItems = value
  }
}
