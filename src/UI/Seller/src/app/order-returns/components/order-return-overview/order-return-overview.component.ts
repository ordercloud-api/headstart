import { Component, Input, OnChanges } from '@angular/core'
import { faUserAlt } from '@fortawesome/free-solid-svg-icons'
import { HSOrder, HSOrderReturn } from '@ordercloud/headstart-sdk'

@Component({
  selector: 'order-return-overview',
  templateUrl: './order-return-overview.component.html',
  styleUrls: ['./order-return-overview.component.scss'],
})
export class OrderReturnOverviewComponent implements OnChanges {
  @Input() order: HSOrder
  @Input() orderReturn: HSOrderReturn
  faUser = faUserAlt
  orderAvatarInitials: string

  ngOnChanges(): void {
    this.setOrderAvatarInitials()
  }

  setOrderAvatarInitials(): void {
    this.orderAvatarInitials = `${this.order?.FromUser?.FirstName?.slice(
      0,
      1
    ).toUpperCase()}${this.order?.FromUser?.LastName?.slice(
      0,
      1
    ).toUpperCase()}`
  }
}
