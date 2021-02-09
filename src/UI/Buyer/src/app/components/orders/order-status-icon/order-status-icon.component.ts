import { Component, Input } from '@angular/core'
import { faCircle, faClock, faBan } from '@fortawesome/free-solid-svg-icons'
import { LineItemStatus } from 'src/app/models/line-item.types'
import { ClaimStatus, HeadstartOrderStatus } from 'src/app/models/order.types'
import { ShippingStatus } from 'src/app/models/shipping.types'

@Component({
  templateUrl: './order-status-icon.component.html',
  styleUrls: ['./order-status-icon.component.scss'],
})
export class OCMOrderStatusIcon {
  @Input() status: HeadstartOrderStatus
  faCircle = faCircle
  faClock = faClock
  faBan = faBan
  statusIconMapping = {
    [HeadstartOrderStatus.Completed]: this.faCircle,
    [HeadstartOrderStatus.AwaitingApproval]: this.faClock,
    [HeadstartOrderStatus.Open]: this.faCircle,
    [HeadstartOrderStatus.Canceled]: this.faBan,
    [ClaimStatus.Pending]: this.faClock,
    [ClaimStatus.NoClaim]: this.faCircle,
    [ShippingStatus.PartiallyShipped]: this.faCircle,
    [ShippingStatus.Processing]: this.faClock,
    [ShippingStatus.Shipped]: this.faCircle,
    [LineItemStatus.Backordered]: this.faClock,
    [LineItemStatus.Complete]: this.faCircle,
    [LineItemStatus.Returned]: this.faCircle,
    [LineItemStatus.Submitted]: this.faCircle,
    [LineItemStatus.ReturnRequested]: this.faClock,
    [LineItemStatus.CancelRequested]: this.faClock,
  }
}
