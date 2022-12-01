import { Injectable, Pipe, PipeTransform } from '@angular/core'

@Injectable({
  providedIn: 'root',
})
@Pipe({
  name: 'lineitemStatus',
})
export class LineItemStatusPipe implements PipeTransform {
  transform(value: string): string {
    if (value === 'Complete') {
      return 'COMMON.LINE_ITEM_STATUS.COMPLETE'
    } else if (value === 'Submitted') {
      return 'COMMON.LINE_ITEM_STATUS.SUBMITTED'
    } else if (value === 'Open') {
      return 'COMMON.LINE_ITEM_STATUS.OPEN'
    } else if (value === 'Backordered') {
      return 'COMMON.LINE_ITEM_STATUS.BACK_ORDERED'
    } else {
      return value
    }
  }
}
