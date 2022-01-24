import { Injectable } from '@angular/core'
import { Me, Inventory, PriceSchedule } from 'ordercloud-javascript-sdk'
import { partition as _partition } from 'lodash'
import { HSLineItem, HSMeProduct, HSPriceSchedule } from '@ordercloud/headstart-sdk'
import { TempSdk } from '../temp-sdk/temp-sdk.service'
import { OrderReorderResponse } from 'src/app/models/order.types'

@Injectable({
  providedIn: 'root',
})
export class ReorderHelperService {
  constructor(private tempSdk: TempSdk) {}

  public async validateReorder(
    orderID: string,
    lineItems: HSLineItem[]
  ): Promise<OrderReorderResponse> {
    // instead of moving all of this logic to the middleware to support orders not
    // submitted by the current user we are adding line items as a paramter

    if (!orderID) throw new Error('Needs Order ID')
    const products = await this.ListProducts(lineItems)
    const [ValidLi, InvalidLi] = _partition(lineItems, (item) =>
      this.isLineItemValid(item, products)
    )
    return { ValidLi: ValidLi as any, InvalidLi: InvalidLi as any }
  }

  private async ListProducts(
    items: HSLineItem[]
  ): Promise<HSMeProduct[]> {
    const productIds = items.map((item) => item.ProductID)
    // TODO - what if the url is too long?
    return (
      await this.tempSdk.listMeProducts({
        filters: { ID: productIds.join('|') },
      })
    ).Items
  }

  private isLineItemValid(item: HSLineItem, products: HSMeProduct[]): boolean {
    const product = products.find(
      (prod) => prod.ID === item.ProductID && prod?.xp?.ProductType !== 'Quote'
    )
    return product && !this.quantityInvalid(item.Quantity, product)
  }

  private quantityInvalid(qty: number, product: HSMeProduct): boolean {
    return (
      this.inventoryTooLow(qty, product.Inventory) ||
      this.restrictedQuantitiesInvalidate(qty, product.PriceSchedule)
    )
  }

  private inventoryTooLow(qty: number, inventory: Inventory): boolean {
    return (
      inventory &&
      inventory.Enabled &&
      !inventory.OrderCanExceed &&
      qty > inventory.QuantityAvailable
    )
  }

  private restrictedQuantitiesInvalidate(
    qty: number,
    schedule: HSPriceSchedule
  ): boolean {
    return (
      schedule.RestrictedQuantity &&
      !schedule.PriceBreaks.some((pb) => pb.Quantity === qty)
    )
  }
}
