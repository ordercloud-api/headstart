import { Pipe, PipeTransform } from '@angular/core'

@Pipe({
  name: 'submittedOrderStatus',
})
export class SubmittedOrderStatusPipe implements PipeTransform {
  transform(value: string): string {
    if (value === 'Open') {
      return 'COMMON.SUBMITTED_ORDER_STATUS.OPEN'
    } else if (value === 'Completed') {
      return 'COMMON.SUBMITTED_ORDER_STATUS.COMPLETED'
    } else if (value === 'Canceled') {
      return 'COMMON.SUBMITTED_ORDER_STATUS.CANCELED'
    } else {
      return value
    }
  }
}
