import { Component, Input, OnChanges } from '@angular/core'
import {
  HSOrder,
  HSOrderReturn,
  ReturnEventDetails,
} from '@ordercloud/headstart-sdk'

type ReturnLog = ReturnEventDetails & { Date: string; Action: string }
@Component({
  selector: 'order-return-logs',
  templateUrl: './order-return-logs.component.html',
  styleUrls: ['./order-return-logs.component.scss'],
})
export class OrderReturnLogsComponent implements OnChanges {
  @Input() orderReturn: HSOrderReturn
  @Input() order: HSOrder
  logs: ReturnLog[] = []

  ngOnChanges(): void {
    if (this.orderReturn?.ID) {
      this.buildLogs()
    }
  }

  private buildLogs(): void {
    this.logs = []
    if (
      this.orderReturn.DateSubmitted &&
      this.orderReturn.xp?.SubmittedStatusDetails
    ) {
      const details = this.orderReturn.xp.SubmittedStatusDetails
      this.logs.push({
        Action: 'Submitted',
        Date: this.orderReturn.DateSubmitted,
        ...details,
      })
    }

    if (
      this.orderReturn.DateApproved &&
      this.orderReturn.xp?.ApprovedStatusDetails
    ) {
      const details = this.orderReturn.xp.ApprovedStatusDetails
      this.logs.push({
        Action: 'Approved',
        Date: this.orderReturn.DateApproved,
        ...details,
      })
    }
    if (
      this.orderReturn.DateDeclined &&
      this.orderReturn.xp?.DeclinedStatusDetails
    ) {
      const details = this.orderReturn.xp.DeclinedStatusDetails
      this.logs.push({
        Action: 'Declined',
        Date: this.orderReturn.DateDeclined,
        ...details,
      })
    }
    if (
      this.orderReturn.DateCompleted &&
      this.orderReturn.xp?.CompletedStatusDetails
    ) {
      const details = this.orderReturn.xp.CompletedStatusDetails
      this.logs.push({
        Action: 'Completed',
        Date: this.orderReturn.DateCompleted,
        ...details,
      })
    }
  }

  getUserLink(log: ReturnLog): string {
    if (!log?.ProcessedByUserId) {
      return ''
    }
    if (log.ProcessedByCompanyId) {
      return `/suppliers/${log.ProcessedByCompanyId}/users/${log.ProcessedByUserId}`
    }
    return `/seller-admin/users/${log.ProcessedByUserId}`
  }
}
