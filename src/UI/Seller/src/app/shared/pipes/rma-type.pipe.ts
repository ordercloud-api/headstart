import { Pipe, PipeTransform } from '@angular/core'

@Pipe({
  name: 'rmaType',
})
export class RmaTypePipe implements PipeTransform {
  transform(value: string): string {
    debugger
    if (value === 'Cancellation') {
      return 'ADMIN.RMAS.CANCELLATION'
    } else if (value === 'Return') {
      return 'ADMIN.RMAS.RETURN'
    } else {
      return value
    }
  }
}
