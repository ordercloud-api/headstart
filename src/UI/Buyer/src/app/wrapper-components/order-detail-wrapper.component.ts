import { Component } from '@angular/core'
import { ActivatedRoute } from '@angular/router'
import { OrderHistoryService } from '../services/order-history/order-history.service'

@Component({
  template: ` <ocm-order-details></ocm-order-details> `,
})
export class OrderDetailWrapperComponent {
  constructor(
    private activatedRoute: ActivatedRoute,
    private orderHistory: OrderHistoryService
  ) {
    this.orderHistory.activeOrderID = this.activatedRoute.snapshot.params.orderID
  }
}
