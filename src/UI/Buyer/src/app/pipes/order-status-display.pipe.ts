import { Pipe, PipeTransform } from '@angular/core'
import { LineItemStatus } from '../models/line-item.types'
import { ClaimStatus, HeadstartOrderStatus } from '../models/order.types'
import { ShippingStatus } from '../models/shipping.types'

@Pipe({
  name: 'orderStatusDisplay',
})
export class OrderStatusDisplayPipe implements PipeTransform {
  OrderStatusMap = {
    [HeadstartOrderStatus.AllSubmitted]: 'All Submitted',
    [HeadstartOrderStatus.AwaitingApproval]: 'Awaiting Approval',
    [HeadstartOrderStatus.ChangesRequested]: 'Changes Requested',
    [HeadstartOrderStatus.Open]: 'Open',
    [HeadstartOrderStatus.Completed]: 'Completed',
    [HeadstartOrderStatus.Canceled]: 'Canceled',
    [ShippingStatus.Shipped]: 'Shipped',
    [ShippingStatus.Backordered]: 'Backordered',
    [ShippingStatus.Processing]: 'Processing',
    [ShippingStatus.PartiallyShipped]: 'Partially Shipped',
    [LineItemStatus.ReturnRequested]: 'Return Requested',
    [LineItemStatus.CancelRequested]: 'Cancel Requested',
    [LineItemStatus.ReturnDenied]: 'Return Denied',
    [LineItemStatus.CancelDenied]: 'Cancel Denied',
    [LineItemStatus.Returned]: 'Returned',
    [LineItemStatus.Complete]: 'Complete',
    [LineItemStatus.Submitted]: 'Submitted',
    [ClaimStatus.Pending]: 'Pending',
    [ClaimStatus.NoClaim]: 'No Claim',
  }

  transform(status: HeadstartOrderStatus): string {
    if (!status) {
      return null
    }
    const displayValue = this.OrderStatusMap[status]
    return displayValue
  }
}
