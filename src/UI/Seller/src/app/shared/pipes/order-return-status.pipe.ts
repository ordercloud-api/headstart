import { Pipe, PipeTransform } from '@angular/core'

@Pipe({
  name: 'orderReturnStatus',
})
export class OrderReturnStatusPipe implements PipeTransform {
  transform(value: string): string {
    if (value === 'Unsubmitted') {
      return 'ADMIN.ORDER_RETURNS.STATUSES.UNSUBMITTED'
    } else if (value === 'AwaitingApproval') {
      return 'ADMIN.ORDER_RETURNS.STATUSES.AWAITING_APPROVAL'
    } else if (value === 'Declined') {
      return 'ADMIN.ORDER_RETURNS.STATUSES.DECLINED'
    } else if (value === 'Open') {
      return 'ADMIN.ORDER_RETURNS.STATUSES.OPEN'
    } else if (value === 'Completed') {
      return 'ADMIN.ORDER_RETURNS.STATUSES.COMPLETED'
    } else if (value === 'Canceled') {
      return 'ADMIN.ORDER_RETURNS.STATUSES.CANCELED'
    } else {
      return value
    }
  }
}
