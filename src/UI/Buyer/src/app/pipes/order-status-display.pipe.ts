import { Pipe, PipeTransform } from '@angular/core'
import { LineItemStatus } from '../models/line-item.types'
import { HeadstartOrderStatus } from '../models/order.types'
import { ShippingStatus } from '../models/shipping.types'

@Pipe({
  name: 'orderStatusDisplay',
})
export class OrderStatusDisplayPipe implements PipeTransform {
  OrderStatusMap = {
    [HeadstartOrderStatus.AllSubmitted]:
      'COMMON.SUBMITTED_ORDER_STATUS.ALL_SUBMITTED',
    [HeadstartOrderStatus.AllQuotes]:
      'COMMON.SUBMITTED_ORDER_STATUS.ALL_QUOTES',
    [HeadstartOrderStatus.AwaitingApproval]:
      'COMMON.SUBMITTED_ORDER_STATUS.AWAITING_APPROVAL',
    [HeadstartOrderStatus.Canceled]: 'COMMON.SUBMITTED_ORDER_STATUS.CANCELED',
    [HeadstartOrderStatus.ChangesRequested]:
      'COMMON.SUBMITTED_ORDER_STATUS.CHANGES_REQUESTED',
    [HeadstartOrderStatus.Completed]: 'COMMON.SUBMITTED_ORDER_STATUS.COMPLETED',
    [HeadstartOrderStatus.Open]: 'COMMON.SUBMITTED_ORDER_STATUS.OPEN',
    [HeadstartOrderStatus.Declined]: 'COMMON.SUBMITTED_ORDER_STATUS.DECLINED',
    [ShippingStatus.Shipped]: 'COMMON.SHIPPING_STATUS.SHIPPED',
    [ShippingStatus.Backordered]: 'COMMON.SHIPPING_STATUS.BACKORDERED',
    [ShippingStatus.Processing]: 'COMMON.SHIPPING_STATUS.PROCESSING',
    [ShippingStatus.PartiallyShipped]:
      'COMMON.SHIPPING_STATUS.PARTIALLY_SHIPPED',
    [LineItemStatus.Complete]: 'COMMON.LINE_ITEM_STATUS.COMPLETE',
    [LineItemStatus.Submitted]: 'COMMON.LINE_ITEM_STATUS.SUBMITTED',
  }

  transform(status: HeadstartOrderStatus): string {
    if (!status) {
      return null
    }
    const displayValue = this.OrderStatusMap[status]
    return displayValue
  }
}
