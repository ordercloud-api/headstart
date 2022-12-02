import { Component, Input, OnChanges } from '@angular/core'
import { OrderProgress, OrderReturnStatus } from '@app-seller/shared'
import { HSOrder, HSOrderReturn } from '@ordercloud/headstart-sdk'

@Component({
  selector: 'order-return-status',
  templateUrl: './order-return-status.component.html',
  styleUrls: ['./order-return-status.component.scss'],
})
export class OrderReturnStatusComponent implements OnChanges {
  @Input() orderReturn: HSOrderReturn
  @Input() order: HSOrder
  orderReturnProgress: OrderProgress

  ngOnChanges(): void {
    this.setProgress()
  }

  setProgress(): void {
    if (!this.orderReturn?.ID) {
      this.orderReturnProgress = {
        StatusDisplay: OrderReturnStatus.AwaitingApproval,
        Value: 25,
        ProgressBarType: 'primary',
        Striped: false,
        Animated: false,
      }
    }
    switch (this.orderReturn?.Status) {
      case OrderReturnStatus.AwaitingApproval:
        this.orderReturnProgress = {
          StatusDisplay: OrderReturnStatus.AwaitingApproval,
          Value: 25,
          ProgressBarType: 'primary',
          Striped: false,
          Animated: false,
        }
        break
      case OrderReturnStatus.Unsubmitted:
        this.orderReturnProgress = {
          StatusDisplay: OrderReturnStatus.Unsubmitted,
          Value: 0,
          ProgressBarType: 'primary',
          Striped: true,
          Animated: true,
        }
        break
      case OrderReturnStatus.Open:
        this.orderReturnProgress = {
          StatusDisplay: OrderReturnStatus.Open,
          Value: 50,
          ProgressBarType: 'primary',
          Striped: true,
          Animated: true,
        }
        break
      case OrderReturnStatus.Completed:
        this.orderReturnProgress = {
          StatusDisplay: OrderReturnStatus.Completed,
          Value: 100,
          ProgressBarType: 'success',
          Striped: false,
          Animated: false,
        }
        break
      case OrderReturnStatus.Declined:
        this.orderReturnProgress = {
          StatusDisplay: OrderReturnStatus.Declined,
          Value: 100,
          ProgressBarType: 'danger',
          Striped: false,
          Animated: false,
        }
        break
      case OrderReturnStatus.Canceled:
        this.orderReturnProgress = {
          StatusDisplay: OrderReturnStatus.Canceled,
          Value: 100,
          ProgressBarType: 'danger',
          Striped: false,
          Animated: false,
        }
        break
    }
  }
}
