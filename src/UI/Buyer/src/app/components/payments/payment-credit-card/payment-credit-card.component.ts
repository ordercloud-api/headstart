import { Component, OnInit, Input, Output, EventEmitter } from '@angular/core'
import { BuyerCreditCard, ListPage } from 'ordercloud-javascript-sdk'
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { CreditCardFormOutput, SelectedCreditCard } from 'src/app/models/credit-card.types'

@Component({
  templateUrl: './payment-credit-card.component.html',
  styleUrls: ['./payment-credit-card.component.scss'],
})
export class OCMPaymentCreditCard implements OnInit {
  @Input() set cards(value: ListPage<BuyerCreditCard>) {
    this._cards = value
    if (value?.Items === null || value?.Items?.length < 1) {
      this._noSavedCards = true
      this._showNewCCForm = true
    } else {
      this._noSavedCards = false
      this._showNewCCForm = false
    }
  }
  @Input() isAnon: boolean
  @Input() paymentError: string
  @Output() cardSelected = new EventEmitter<SelectedCreditCard>()
  _cards: ListPage<BuyerCreditCard>
  _showNewCCForm: boolean
  _noSavedCards: boolean

  form = new FormGroup({
    cardID: new FormControl(null, Validators.required),
    cvv: new FormControl('', Validators.required),
  })

  constructor() { }

  ngOnInit(): void { }

  submit(output: CreditCardFormOutput): void {
    const cardID = this.form.value.cardID
    const SavedCard = this._cards.Items.find((c) => c.ID === cardID)
    this.cardSelected.emit({ SavedCard, CVV: output.cvv, NewCard: output.card })
  }

  toggleShowCCForm(event: any): void {
    this._showNewCCForm = event.target.value === 'new'
  }
}
