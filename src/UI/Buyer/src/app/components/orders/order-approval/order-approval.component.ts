import { Component, OnInit, Input } from '@angular/core'
import { FormGroup, FormControl } from '@angular/forms'
import { ToastrService } from 'ngx-toastr'
import { IntegrationEvents } from 'ordercloud-javascript-sdk'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './order-approval.component.html',
  styleUrls: ['./order-approval.component.scss'],
})
export class OCMOrderApproval implements OnInit {
  approved: boolean
  form: FormGroup
  @Input() orderID: string
  requestedConfirmation = false

  constructor(
    private toasterService: ToastrService,
    private context: ShopperContextService
  ) {}

  ngOnInit(): void {
    this.form = new FormGroup({ comments: new FormControl('') })
  }

  openConfirmation(approved: boolean): void {
    this.approved = approved
    this.requestedConfirmation = true
  }

  closeConfirmation(): void {
    this.requestedConfirmation = false
    this.form.controls.comments.setValue('')
  }

  async submitReview(): Promise<void> {
    const comments = this.form.value.comments
    if (this.approved) {
      await IntegrationEvents.Calculate('Outgoing', this.orderID)
      await this.context.orderHistory.approveOrder(this.orderID, comments)
    } else {
      await this.context.orderHistory.declineOrder(this.orderID, comments)
    }
    this.context.ordersToApprove.reset()
    this.toasterService.success(
      `Order ${this.orderID} was ${this.approved ? 'Approved' : 'Declined'}`
    )
    this.requestedConfirmation = false
    this.context.router.toOrdersToApprove()
  }
}
