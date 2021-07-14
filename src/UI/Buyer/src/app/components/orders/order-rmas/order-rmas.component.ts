import { Component, Input } from '@angular/core'
import { HSLineItem, OrderDetails, RMA } from '@ordercloud/headstart-sdk'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './order-rmas.component.html',
  styleUrls: ['./order-rmas.component.scss'],
})
export class OCMOrderRMAs {
  @Input() set orderDetails(value: OrderDetails) {
    if (value) {
      this._orderDetails = value
      void this.init(value?.Order?.ID)
    }
  }
  _orderDetails: OrderDetails
  rmas: RMA[]
  rmaToOrderLineItemMap: Map<string, HSLineItem[]>
  someLineItems: HSLineItem[]

  constructor(private context: ShopperContextService) {}

  async init(orderID: string): Promise<void> {
    const rmaListPage = await this.context.rmaService.listRMAsForOrder(orderID)
    this.rmas = rmaListPage.Items
    this.buildRMAToOrderLineItemMap()
  }

  buildRMAToOrderLineItemMap(): void {
    const rmaToOrderLineItemMap = new Map<string, HSLineItem[]>()
    this.rmas?.forEach((rma) => {
      const orderLineItemsForRMA = []
      rma?.LineItems?.forEach((rmaLineItem) => {
        const matchingLineItem = this._orderDetails?.LineItems?.find(
          (orderLineItem) => orderLineItem?.ID === rmaLineItem?.ID
        )
        orderLineItemsForRMA.push(matchingLineItem)
      })
      rmaToOrderLineItemMap.set(rma?.RMANumber, orderLineItemsForRMA)
    })
    this.rmaToOrderLineItemMap = rmaToOrderLineItemMap
  }
}
