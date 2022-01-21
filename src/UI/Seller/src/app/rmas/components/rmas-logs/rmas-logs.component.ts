import { Component, Input, OnChanges, SimpleChanges } from '@angular/core'
import { HSOrder, RMA } from '@ordercloud/headstart-sdk'
import {
  AdminUsers,
  MeUser,
  SupplierUsers,
  User,
} from 'ordercloud-javascript-sdk'

@Component({
  selector: 'rmas-logs-component',
  templateUrl: './rmas-logs.component.html',
  styleUrls: ['./rmas-logs.component.scss'],
})
export class RMALogsComponent implements OnChanges {
  @Input()
  set selectedRMA(rma: RMA) {
    this._rma = rma
    void this.getUsersFromLogs()
  }
  @Input() relatedOrder: HSOrder
  @Input() currentUser: MeUser
  _rma: RMA
  usersFromLogs: User[] = []
  constructor() {}

  async ngOnChanges(changes: SimpleChanges): Promise<void> {
    if (
      changes.selectedRMA?.currentValue !== changes.selectedRMA?.previousValue
    ) {
      await this.getUsersFromLogs()
    }
  }

  async getUsersFromLogs(): Promise<void> {
    if (this._rma?.Logs?.length) {
      for (const log of this._rma?.Logs) {
        if (!this.usersFromLogs?.some((user) => user?.ID === log?.FromUserID)) {
          let user: User
          if (this._rma.SupplierID) {
            user = await SupplierUsers.Get(
              this._rma?.SupplierID,
              log?.FromUserID
            )
          } else {
            user = await AdminUsers.Get(log?.FromUserID)
          }
          this.usersFromLogs.push(user)
        }
      }
    }
  }

  findUser(userID: string): string {
    const user = this.usersFromLogs?.find((user) => user.ID === userID)
    return `${user?.LastName}, ${user?.FirstName}`
  }

  wasRefundIssued(refund: number): string {
    return refund > 0 ? 'Y' : 'N'
  }
}
