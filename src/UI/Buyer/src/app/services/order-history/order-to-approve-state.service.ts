import { Injectable } from '@angular/core'
import { BehaviorSubject } from 'rxjs'
import { Me } from 'ordercloud-javascript-sdk'

@Injectable({
  providedIn: 'root',
})
export class OrdersToApproveStateService {
  public showAlert = new BehaviorSubject<number>(0)
  public numberOfOrdersToApprove = new BehaviorSubject<number>(0)

  constructor() {}

  async reset(): Promise<void> {
    const ordersToApproverResponse = await Me.ListApprovableOrders()
    this.numberOfOrdersToApprove.next(ordersToApproverResponse.Meta.TotalCount)
  }

  async alertIfOrdersToApprove(): Promise<void> {
    await this.reset()
    if (this.numberOfOrdersToApprove.value) {
      this.showAlert.next(this.numberOfOrdersToApprove.value)
    }
  }
}
