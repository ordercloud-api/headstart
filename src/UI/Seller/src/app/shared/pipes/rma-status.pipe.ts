import { Pipe, PipeTransform } from '@angular/core'

@Pipe({
  name: 'rmaStatus',
})
export class RmaStatusPipe implements PipeTransform {
  transform(value: string): string {
    if (value === 'Requested') {
      return 'ADMIN.RMAS.REQUESTED'
    } else if (value === 'Denied') {
      return 'ADMIN.RMAS.DENIED'
    } else if (value === 'Processing') {
      return 'ADMIN.RMAS.PROCESSING'
    } else if (value === 'Approved') {
      return 'ADMIN.RMAS.APPROVED'
    } else if (value === 'Complete') {
      return 'ADMIN.RMAS.COMPLETE'
    } else {
      return value
    }
  }
}
