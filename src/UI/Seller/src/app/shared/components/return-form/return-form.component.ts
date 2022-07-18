import { Component, Input, OnInit, Output, EventEmitter } from '@angular/core'
import { FormControl, FormGroup } from '@angular/forms'
import { LineItemStatus } from '@app-seller/models/order.types'
import { NgbModal } from '@ng-bootstrap/ng-bootstrap'
import {
  LineItem,
  LineItems,
  Orders,
  Order,
  OrderDirection,
} from 'ordercloud-javascript-sdk'

@Component({
  selector: 'return-form',
  templateUrl: './return-form.component.html',
})
export class ReturnForm implements OnInit {
  returnForm: FormGroup
  isSaving = false
  @Input() order: Order
  @Input() lineItem: LineItem
  @Output() lineItemUpdate = new EventEmitter()

  constructor(private modalService: NgbModal) {}

  ngOnInit() {
    this.setReturnForm()
  }

  setReturnForm() {
    this.returnForm = new FormGroup({
      Comment: new FormControl(
        this.lineItem.xp?.LineItemReturnInfo?.Comment || ''
      ),
      Complete: new FormControl(null),
    })
  }

  getIncomingOrOutgoing(): OrderDirection {
    if (window.location.href.includes('Outgoing')) {
      return 'Outgoing'
    }
    return 'Incoming'
  }

  open(content) {
    this.modalService.open(content, { ariaLabelledBy: 'return-form' })
  }

  async onReturnFormSubmit(): Promise<void> {
    this.isSaving = true
    const orderDirection = this.getIncomingOrOutgoing()
    const comment = this.returnForm.value.Comment
    const status = this.returnForm.value.Complete
      ? LineItemStatus.Returned
      : LineItemStatus.ReturnRequested
    const resolved = this.returnForm.value.Complete
    await Orders.Patch(orderDirection, this.order.ID, {
      xp: { OrderReturnInfo: { Resolved: resolved } },
    })
    await LineItems.Patch(orderDirection, this.order.ID, this.lineItem.ID, {
      xp: {
        LineItemReturnInfo: { Resolved: resolved, Comment: comment },
        LineItemStatus: status,
      },
    })
    this.isSaving = false
    this.lineItemUpdate.emit()
  }
}
