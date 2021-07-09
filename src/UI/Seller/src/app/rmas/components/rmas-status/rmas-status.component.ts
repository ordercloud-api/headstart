import { Component, Input } from '@angular/core'
import { OrderProgress, RMAStatus } from '@app-seller/shared'
import { Order } from '@ordercloud/angular-sdk'
import { RMA } from '@ordercloud/headstart-sdk'

@Component({
  selector: 'rmas-status-component',
  templateUrl: './rmas-status.component.html',
  styleUrls: ['./rmas-status.component.scss'],
})
export class RMAStatusComponent {
  @Input() set rma(value: RMA) {
    this._rma = value
    this.setRMAProgress(value)
  }
  @Input() isSellerUser: boolean
  @Input() buyerOrderData: Order
  @Input() supplierOrderData: Order
  rmaProgress: OrderProgress = {
    StatusDisplay: 'Processing',
    Value: 25,
    ProgressBarType: 'primary',
    Striped: false,
    Animated: false,
  }
  _rma: RMA

  setRMAProgress(rma: RMA): void {
    switch (rma?.Status) {
      case RMAStatus.Requested:
        this.rmaProgress = {
          StatusDisplay: RMAStatus.Requested,
          Value: 25,
          ProgressBarType: 'primary',
          Striped: false,
          Animated: false,
        }
        break
      case RMAStatus.Processing:
        this.rmaProgress = {
          StatusDisplay: RMAStatus.Processing,
          Value: 50,
          ProgressBarType: 'primary',
          Striped: true,
          Animated: true,
        }
        break
      case RMAStatus.Approved:
        this.rmaProgress = {
          StatusDisplay: RMAStatus.Approved,
          Value: 75,
          ProgressBarType: 'primary',
          Striped: true,
          Animated: true,
        }
        break
      case RMAStatus.Complete:
        this.rmaProgress = {
          StatusDisplay: RMAStatus.Complete,
          Value: 100,
          ProgressBarType: 'success',
          Striped: false,
          Animated: false,
        }
        break
      case RMAStatus.Denied:
        this.rmaProgress = {
          StatusDisplay: RMAStatus.Denied,
          Value: 100,
          ProgressBarType: 'danger',
          Striped: false,
          Animated: false,
        }
        break
    }
  }

  buildOrderDetailsRoute(): string {
    return this.isSellerUser
      ? `/orders/${this.buyerOrderData.ID}`
      : `/orders/${this.supplierOrderData.ID}`
  }
}
