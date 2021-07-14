import { Component, Input } from '@angular/core'
import { faUserAlt } from '@fortawesome/free-solid-svg-icons'
import { Order } from '@ordercloud/angular-sdk'
import { RMA } from '@ordercloud/headstart-sdk'

@Component({
  selector: 'rmas-overview-component',
  templateUrl: './rmas-overview.component.html',
  styleUrls: ['./rmas-overview.component.scss'],
})
export class RMAOverviewComponent {
  @Input() set buyerOrderData(value: Order) {
    this._buyerOrderData = value
    this.setOrderAvatarInitials()
  }
  @Input() rma: RMA
  @Input() isSellerUser: boolean
  _buyerOrderData: Order
  faUser = faUserAlt
  orderAvatarInitials: string

  setOrderAvatarInitials(): void {
    this.orderAvatarInitials = `${this.buyerOrderData?.FromUser?.FirstName?.slice(
      0,
      1
    ).toUpperCase()}${this.buyerOrderData?.FromUser?.LastName?.slice(
      0,
      1
    ).toUpperCase()}`
  }
}
