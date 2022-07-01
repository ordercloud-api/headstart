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
    } else if (value === 'Canceled') {
      return 'COMMON.LINE_ITEM_STATUS.CANCELED'
    } else if (value === 'CancelRequested') {
      return 'COMMON.LINE_ITEM_STATUS.CANCEL_REQUESTED'
    } else if (value === 'CancelDenied') {
      return 'COMMON.LINE_ITEM_STATUS.CANCEL_DENIED'
    } else if (value === 'Returned') {
      return 'COMMON.LINE_ITEM_STATUS.RETURNED'
    } else if (value === 'ReturnRequested') {
      return 'COMMON.LINE_ITEM_STATUS.RETURN_REQUESTED'
    } else if (value === 'ReturnDenied') {
      return 'COMMON.LINE_ITEM_STATUS.RETURN_DENIED'
    } else {
      return value
    }
  }
}
