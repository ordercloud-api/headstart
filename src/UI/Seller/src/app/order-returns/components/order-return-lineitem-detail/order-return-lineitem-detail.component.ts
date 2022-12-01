import { Component, Input, Output, EventEmitter } from '@angular/core'
import { getPrimaryLineItemImage } from '@app-seller/shared/services/assets/asset.helper'
import {
  LineItem,
  LineItemSpec,
  OrderReturnItem,
} from 'ordercloud-javascript-sdk'
import { HSLineItem, HSOrder, HSOrderReturn } from '@ordercloud/headstart-sdk'
import { RefundInputChange } from '../order-return-refund-input/order-return-refund-input.component'

@Component({
  selector: 'order-return-lineitem-detail',
  templateUrl: './order-return-lineitem-detail.component.html',
  styleUrls: ['./order-return-lineitem-detail.component.scss'],
})
export class OrderReturnLineItemDetailComponent {
  @Input() lineItems: HSLineItem[]
  @Input() loading: boolean
  @Input() orderReturn: HSOrderReturn
  @Input() order: HSOrder
  @Output() lineItemRefundChange = new EventEmitter<RefundInputChange>()

  getImageUrl(lineItemID: string): string {
    return getPrimaryLineItemImage(lineItemID, this.lineItems)
  }

  getVariableTextSpecs = (li: LineItem): LineItemSpec[] =>
    li?.Specs?.filter((s) => s.OptionID === null)

  getOrderReturnItem(lineItem: HSLineItem): OrderReturnItem {
    return this.orderReturn.ItemsToReturn.find(
      (item) => item.LineItemID === lineItem.ID
    )
  }
}
