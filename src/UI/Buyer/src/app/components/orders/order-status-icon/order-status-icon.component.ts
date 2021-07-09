import { Component, Input } from '@angular/core'
import { faCircle, faClock, faBan } from '@fortawesome/free-solid-svg-icons'
import { LineItemStatus } from 'src/app/models/line-item.types'
import {
  ClaimStatus,
  HeadstartOrderStatus,
  RMAStatus,
} from 'src/app/models/order.types'
import { ShippingStatus } from 'src/app/models/shipping.types'

@Component({
  templateUrl: './order-status-icon.component.html',
  styleUrls: ['./order-status-icon.component.scss'],
})
export class OCMOrderStatusIcon {
  @Input() status:
    | HeadstartOrderStatus
    | ClaimStatus
    | ShippingStatus
    | LineItemStatus
  faCircle = faCircle
  faClock = faClock
  faBan = faBan
  statusIconMapping = {
    [HeadstartOrderStatus.Completed]: this.faCircle,
    [HeadstartOrderStatus.AwaitingApproval]: this.faClock,
    [HeadstartOrderStatus.Open]: this.faCircle,
    [HeadstartOrderStatus.Canceled]: this.faCircle,
    [ClaimStatus.Pending]: this.faClock,
    [ClaimStatus.NoClaim]: this.faCircle,
    [ShippingStatus.PartiallyShipped]: this.faCircle,
    [ShippingStatus.Processing]: this.faClock,
    [ShippingStatus.Shipped]: this.faCircle,
    [LineItemStatus.Canceled]: this.faCircle,
    [LineItemStatus.Backordered]: this.faClock,
    [LineItemStatus.Complete]: this.faCircle,
    [LineItemStatus.Returned]: this.faCircle,
    [LineItemStatus.Submitted]: this.faCircle,
    [LineItemStatus.ReturnRequested]: this.faClock,
    [LineItemStatus.CancelRequested]: this.faClock,
    [LineItemStatus.ReturnDenied]: this.faBan,
    [LineItemStatus.CancelDenied]: this.faBan,
    [RMAStatus.Requested]: this.faClock,
    [RMAStatus.Denied]: this.faBan,
    [RMAStatus.Processing]: this.faClock,
    [RMAStatus.Approved]: this.faCircle,
    [RMAStatus.Complete]: this.faCircle,
    [RMAStatus.Canceled]: this.faCircle,
    [RMAStatus.PartialQtyApproved]: this.faCircle,
    [RMAStatus.PartialQtyComplete]: this.faCircle,
  }
}
