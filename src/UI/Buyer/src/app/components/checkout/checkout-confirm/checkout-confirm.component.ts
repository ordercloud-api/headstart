import { Component, OnInit, Output, EventEmitter, Input } from '@angular/core'
import { Payment, ListPage } from 'ordercloud-javascript-sdk'
import { FormGroup, FormControl } from '@angular/forms'
import {
  HSOrder,
  HSLineItem,
} from '@ordercloud/headstart-sdk'

@Component({
  templateUrl: './checkout-confirm.component.html',
  styleUrls: ['./checkout-confirm.component.scss'],
})
export class OCMCheckoutConfirm implements OnInit {
  form: FormGroup
  isSubmittingOrder = false // prevent double-click submits

  @Input() isAnon: boolean
  @Input() order: HSOrder
  @Input() lineItems: ListPage<HSLineItem>
  @Input() payments: Payment[]
  @Output() submitOrderWithComment = new EventEmitter<string>()

  constructor() { }

  ngOnInit(): void {
    this.form = new FormGroup({ comments: new FormControl('') })
  }

  saveCommentsAndSubmitOrder(): void {
    this.isSubmittingOrder = true
    const Comments = this.form.get('comments').value
    this.submitOrderWithComment.emit(Comments)
  }
}
