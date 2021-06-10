import {
  Payment,
  Orders,
  BuyerAddress,
  Me,
  OrderWorksheet,
  IntegrationEvents,
  ShipMethodSelection,
  LineItems,
  Suppliers,
  SupplierAddresses,
  Address,
} from 'ordercloud-javascript-sdk'
import { Injectable } from '@angular/core'
import { PaymentHelperService } from '../payment-helper/payment-helper.service'
import { OrderStateService } from './order-state.service'
import {
  HeadStartSDK,
  HSOrder,
  ListPage,
  HSLineItem,
  HSAddressBuyer,
} from '@ordercloud/headstart-sdk'
import { isEqual, max, uniqWith } from 'lodash'
import { LineItemGroupSupplier } from 'src/app/models/line-item.types'
import { AddressType } from 'src/app/models/checkout.types'
import { AppConfig } from 'src/app/models/environment.types'

@Injectable({
  providedIn: 'root',
})
export class CheckoutService {
  constructor(
    private paymentHelper: PaymentHelperService,
    private state: OrderStateService,
    private appConfig: AppConfig
  ) {}

  async setShippingAddress(address: BuyerAddress): Promise<HSOrder> {
    // If a saved address (with an ID) is changed by the user it is attached to an order as a one time address.
    // However, order.ShippingAddressID (or BillingAddressID) still points to the unmodified address. The ID should be cleared.
    // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
    ;(address as any).ID = null
    this.order = await HeadStartSDK.ValidatedAddresses.SetShippingAddress(
      'Outgoing',
      this.order.ID,
      address as Address
    )
    return this.order
  }

  async setShippingAddressByID(address: HSAddressBuyer): Promise<HSOrder> {
    try {
      await Orders.Patch('Outgoing', this.order.ID, {
        xp: { ShippingAddress: address },
      })
      return await this.patch({ ShippingAddressID: address.ID })
    } catch (ex) {
      if (ex?.errors?.Errors?.[0]?.ErrorCode === 'NotFound') {
        if (ex?.errors?.Errors?.[0]?.Data.ObjectType === 'Order') {
          throw Error('You no longer have access to this order')
        }
        throw Error(
          'You no longer have access to this saved address. Please enter or select a different one.'
        )
      }
      throw ex
    }
  }

  async setOneTimeAddress(
    address: Address,
    addressType: AddressType
  ): Promise<void> {
    delete address.ID
    addressType === 'shipping'
      ? await Orders.SetShippingAddress('Outgoing', this.order.ID, address)
      : await Orders.SetBillingAddress('Outgoing', this.order.ID, address)
  }

  async setBuyerLocationByID(buyerLocationID: string): Promise<HSOrder> {
    const patch = {
      BillingAddressID: buyerLocationID,
      xp: { ApprovalNeeded: '' },
    }
    const isApprovalNeeded = await this.isApprovalNeeded(buyerLocationID)
    if (isApprovalNeeded) patch.xp.ApprovalNeeded = buyerLocationID
    try {
      this.order = await this.patch(patch as HSOrder)
      return this.order
    } catch (ex) {
      if (ex.errors.Errors[0].ErrorCode === 'NotFound') {
        if (ex?.errors?.Errors?.[0]?.Data.ObjectType === 'Order') {
          throw Error('You no longer have access to this order')
        }
        throw Error(
          'You no longer have access to this buyer location. Please enter or select a different one.'
        )
      }
    }
  }

  async isApprovalNeeded(locationID: string): Promise<boolean> {
    const userGroups = await Me.ListUserGroups({
      searchOn: ['ID'],
      search: locationID,
    })
    return userGroups.Items.some((u) => u.ID === `${locationID}-NeedsApproval`)
  }

  async listPayments(): Promise<ListPage<Payment>> {
    return await this.paymentHelper.ListPaymentsOnOrder(this.order.ID)
  }

  // Integration Methods
  // order cloud sandbox service methods, to be replaced by updated sdk in the future
  async estimateShipping(): Promise<OrderWorksheet> {
    return await IntegrationEvents.EstimateShipping('Outgoing', this.order.ID)
  }

  async cleanLineItemIDs(
    orderID: string,
    lineItems: HSLineItem[]
  ): Promise<void> {
    /* line item ids are significant for suppliers creating a relationship
     * between their shipments and line items in ordercloud
     * we are sequentially labeling these ids for ease of shipping */
    const patchQueue: Promise<any>[] = [] //This is an array of operations we will add to.
    const unIndexedLi = lineItems.filter((li) => li.ID.length != 4)
    let indexedLi = lineItems.filter((li) => li.ID.length === 4)
    lineItems.forEach((li, index) => {
      if (
        !lineItems.map((li) => li.ID).includes(this.createIDFromIndex(index))
      ) {
        //first check if there are any that have not been idexed at all
        if (unIndexedLi.length) {
          const liToPatch = unIndexedLi[0]
          patchQueue.push(
            LineItems.Patch('Outgoing', orderID, liToPatch.ID, {
              ID: this.createIDFromIndex(index),
            })
          )
          unIndexedLi.shift() //now remove that li from the unIndexed array
        } else {
          //otherwise grab the highest indexed line item and patch that to fill the hole.
          const maxIndex = max(
            indexedLi.map((li) => Number(li.ID.substring(1)))
          )
          const indextoMatch = this.createIDFromIndex(maxIndex - 1)
          const liToPatch = indexedLi.find((li) => {
            return li.ID === indextoMatch
          })
          patchQueue.push(
            LineItems.Patch('Outgoing', orderID, liToPatch.ID, {
              ID: this.createIDFromIndex(index),
            })
          )
          indexedLi = indexedLi.filter((li) => li.ID !== indextoMatch) // now remove that from indexedLi array so we dont patch it again
        }
      }
    })
    await Promise.all(patchQueue)
    await this.state.resetLineItems()
  }

  createIDFromIndex(index: number): string {
    /* X was choosen as a prefix for the lineItem ID so that it is easy to
     * direct suppliers where to look for the ID. L and I are sometimes indistinguishable
     * from the number 1 so I avoided those. X is also difficult to confuse with other
     * letters when verbally pronounced */
    const countInList = index + 1
    const paddedCount = countInList.toString().padStart(3, '0')
    return 'X' + paddedCount
  }

  async selectShipMethods(
    selections: ShipMethodSelection[]
  ): Promise<OrderWorksheet> {
    const orderWorksheet = await IntegrationEvents.SelectShipmethods(
      'Outgoing',
      this.order.ID,
      {
        ShipMethodSelections: selections,
      }
    )
    this.order = orderWorksheet.Order
    return orderWorksheet
  }

  async buildSupplierData(
    lineItems: HSLineItem[]
  ): Promise<LineItemGroupSupplier[]> {
    const supplierData = lineItems?.map((li) => ({
      supplierID: li?.SupplierID,
      ShipFromAddressID: li?.ShipFromAddressID,
    }))
    const uniqueSuppliers = uniqWith(supplierData, isEqual)
    const supplierIDs = uniqueSuppliers.map((s) => s.supplierID)
    const suppliers = await Suppliers.List({
      filters: { ID: supplierIDs.join('|') },
    })
    const supplierItems: LineItemGroupSupplier[] = []
    for (const combo of uniqueSuppliers) {
      if (combo.supplierID === null) {
        //This handles seller owned products
        supplierItems.push(
          this.buildSellerShipmentData(combo.ShipFromAddressID)
        )
      } else {
        const supplier = suppliers.Items.find((s) => s.ID === combo.supplierID)
        if (combo.supplierID && combo.ShipFromAddressID) {
          const shipFrom = await SupplierAddresses.Get(
            combo.supplierID,
            combo.ShipFromAddressID
          )
          supplierItems.push({ supplier, shipFrom })
        } else {
          supplierItems.push({ supplier, shipFrom: null })
        }
      }
    }
    return supplierItems
  }

  buildSellerShipmentData(shipFromAddressID: string): LineItemGroupSupplier {
    return {
      supplier: {
        ID: null,
        Name: this.appConfig.sellerName || 'Purchasing from Seller',
      },
      shipFrom: {
        ID: shipFromAddressID,
      },
    }
  }

  async calculateOrder(): Promise<HSOrder> {
    const orderCalculation = await IntegrationEvents.Calculate(
      'Outgoing',
      this.order.ID
    )
    this.order = orderCalculation.Order
    return this.order
  }

  public async patch(order: HSOrder, orderID?: string): Promise<HSOrder> {
    this.order = (await Orders.Patch(
      'Outgoing',
      orderID || this.order.ID,
      order
    )) as HSOrder
    return this.order
  }

  private get order(): HSOrder {
    return this.state.order
  }

  private set order(value: HSOrder) {
    this.state.order = value
  }
}
