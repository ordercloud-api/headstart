import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core'
import { BuyerCreditCard, ListPage } from 'ordercloud-javascript-sdk'
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { CreditCardFormOutput, SelectedCreditCard } from 'src/app/models/credit-card.types'

@Component({
  templateUrl: './payment-credit-card.component.html',
  styleUrls: ['./payment-credit-card.component.scss'],
})
export class OCMPaymentCreditCard implements OnInit {
  @Input() cards: ListPage<BuyerCreditCard>
  @Input() termsAccepted: boolean
  @Input() paymentError: string
  @Output() cardSelected = new EventEmitter<SelectedCreditCard>()
  showNewCCForm = false

  form = new FormGroup({
    cardID: new FormControl(null, Validators.required),
    cvv: new FormControl('', Validators.required),
  })

  constructor() {}

  ngOnInit(): void {}

  submit(output: CreditCardFormOutput): void {
    const cardID = this.form.value.cardID
    const SavedCard = this.cards.Items.find((c) => c.ID === cardID)
    this.cardSelected.emit({ SavedCard, CVV: output.cvv, NewCard: output.card })
  }

  toggleShowCCForm(event: any): void {
    this.showNewCCForm = event.target.value === 'new'
  }
}
