import { Component, Input, OnChanges, SimpleChanges } from '@angular/core'
import { HSLineItem, HSOrder, HSOrderReturn } from '@ordercloud/headstart-sdk'
import {
  LineItems,
  OrderReturnApproval,
  OrderReturns,
  Orders,
} from 'ordercloud-javascript-sdk'

@Component({
  selector: 'order-return-edit',
  templateUrl: './order-return-edit.component.html',
  styleUrls: ['./order-return-edit.component.scss'],
})
export class OrderReturnEditComponent implements OnChanges {
  @Input() orderReturn: HSOrderReturn
  order: HSOrder
  lineItems: HSLineItem[] = []
  orderReturnApproval: OrderReturnApproval

  ngOnChanges(): void {
    if (this.orderReturn?.ID) {
      this.getRelatedData()
    }
  }

  async getRelatedData(): Promise<void> {
    this.lineItems = []
    const [order, lineItems, returnApprovalList] = await Promise.all([
      await Orders.Get('All', this.orderReturn.OrderID),
      await this.getRelatedLineItems(this.orderReturn),
      await OrderReturns.ListApprovals(this.orderReturn.ID, {
        filters: { Status: 'Approved|Declined' },
      }),
    ])

    this.order = order
    this.lineItems = lineItems
    this.orderReturnApproval = returnApprovalList.Items.length
      ? returnApprovalList.Items[0]
      : null
  }

  async getRelatedLineItems(orderReturn: HSOrderReturn): Promise<HSLineItem[]> {
    const lineItemIds = orderReturn.ItemsToReturn.map((item) => item.LineItemID)
    const lineItemList = await LineItems.List('All', orderReturn.OrderID, {
      pageSize: 100,
      filters: { ID: lineItemIds.join('|') },
    })
    return lineItemList.Items
  }

  updateReturn(event: HSOrderReturn): void {
    this.orderReturn = event
  }
}
