import { Input, Component } from '@angular/core'
import { groupBy as _groupBy, isEqual, uniqWith } from 'lodash'
import { HSLineItem, HSOrderReturn } from '@ordercloud/headstart-sdk'
import { getPrimaryLineItemImage } from 'src/app/services/images.helpers'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { LineItemGroupSupplier } from 'src/app/models/line-item.types'
import { CheckoutService } from 'src/app/services/order/checkout.service'
import { Address, OrderReturnItem } from 'ordercloud-javascript-sdk'

@Component({
  templateUrl: './order-returns-lineitem-table.html',
  styleUrls: ['./order-returns-lineitem-table.scss'],
})
export class OCMReturnsLineitemTable {
  @Input() set lineItems(lineItems: HSLineItem[]) {
    this._lineItems = lineItems
    this.initLineItems() // if line items change we need to regroup them
    this.setSupplierData()
  }
  @Input() orderReturn: HSOrderReturn

  supplierArray: any[]
  suppliers: LineItemGroupSupplier[]
  liGroupedByShipFrom: HSLineItem[][]
  _lineItems: HSLineItem[] = []
  orderCurrency: string

  constructor(
    public context: ShopperContextService,
    private checkoutService: CheckoutService
  ) {
    this.orderCurrency = this.context.currentUser.get().Currency
  }

  shouldDisplayAddress(shipFrom: Partial<Address>): boolean {
    return shipFrom?.Street1 && shipFrom.Street1 !== null
  }

  initLineItems(): void {
    if (!this._lineItems || !this._lineItems.length) {
      return
    }
    this.liGroupedByShipFrom = this.groupLineItemsByShipFrom(this._lineItems)
  }

  async setSupplierData(): Promise<void> {
    const supplierArray = uniqWith(
      this._lineItems?.map((li) => ({
        supplierID: li?.SupplierID,
        ShipFromAddressID: li?.ShipFromAddressID,
      })),
      isEqual
    )
    if (JSON.stringify(supplierArray) !== JSON.stringify(this.supplierArray)) {
      this.supplierArray = supplierArray
      const supplierList = await this.checkoutService.buildSupplierData(
        this._lineItems
      )
      this.buildSupplierArray(supplierList)
    }
  }

  buildSupplierArray(supplierList: LineItemGroupSupplier[]): void {
    const suppliers: LineItemGroupSupplier[] = []
    if (this.liGroupedByShipFrom) {
      this.liGroupedByShipFrom.forEach((group) => {
        suppliers.push(
          supplierList.find((s) => s.shipFrom.ID === group[0].ShipFromAddressID)
        )
      })
    }
    this.suppliers = suppliers
  }

  groupLineItemsByShipFrom(lineItems: HSLineItem[]): HSLineItem[][] {
    const liGroups = _groupBy(lineItems, (li) => li.ShipFromAddressID)
    return Object.values(liGroups).sort((a, b) => {
      const nameA = a[0]?.ShipFromAddressID?.toUpperCase() // ignore upper and lowercase
      const nameB = b[0]?.ShipFromAddressID?.toUpperCase() // ignore upper and lowercase
      return nameA.localeCompare(nameB)
    })
  }

  toProductDetails(productID: string): void {
    this.context.router.toProductDetails(productID)
  }

  getImageUrl(lineItemID: string): string {
    return getPrimaryLineItemImage(
      lineItemID,
      this._lineItems,
      this.context.currentUser.get()
    )
  }

  getItemToReturnReason(lineItem: HSLineItem): string {
    const item = this.getItemToReturn(lineItem)
    return item.Comments
  }

  getItemToReturnQuantity(lineItem: HSLineItem): number {
    const item = this.getItemToReturn(lineItem)
    return item.Quantity || 0
  }

  getItemToReturnRefundTotal(lineItem: HSLineItem): number {
    const item = this.getItemToReturn(lineItem)
    return item.RefundAmount || 0
  }

  private getItemToReturn(lineItem: HSLineItem): OrderReturnItem {
    return this.orderReturn.ItemsToReturn.find(
      (item) => item.LineItemID === lineItem.ID
    )
  }
}
