import { Component, Input } from '@angular/core'
import { HSLineItem, OrderDetails } from '@ordercloud/headstart-sdk'
import { OrderReturnItem } from 'ordercloud-javascript-sdk'

@Component({
  templateUrl: './order-returns.component.html',
  styleUrls: ['./order-returns.component.scss'],
})
export class OCMOrderReturns {
  @Input() set orderDetails(value: OrderDetails) {
    if (value) {
      this._orderDetails = value
      void this.init()
    }
  }
  _orderDetails: OrderDetails
  lineItemMap: Map<string, OrderReturnItem[]>

  init(): void {
    this.buildLineItemMap()
  }

  buildLineItemMap(): void {
    const map = new Map<string, HSLineItem[]>()
    this._orderDetails.OrderReturns?.forEach((orderReturn) => {
      const lineItems = orderReturn.ItemsToReturn.map(
        (orderReturnItem) => orderReturnItem.LineItemID
      )
        .map((lineItemId) =>
          this._orderDetails.LineItems.find((li) => li.ID === lineItemId)
        )
        .filter((lineItem) => Boolean(lineItem)) // filter out unfound line items

      map.set(orderReturn.ID, lineItems)
    })
    this.lineItemMap = map
  }
}
