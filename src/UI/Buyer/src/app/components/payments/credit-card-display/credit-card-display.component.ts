import { Component, Input } from '@angular/core'
import { BuyerCreditCard, Address } from 'ordercloud-javascript-sdk'
import { HSBuyerCreditCard } from 'src/app/models/credit-card.types'

@Component({
  templateUrl: './credit-card-display.component.html',
  styleUrls: ['./credit-card-display.component.scss'],
})
export class OCMCreditCardDisplay {
  @Input() set card(value: HSBuyerCreditCard) {
    this.creditCard = value
    this.address = value?.xp?.CCBillingAddress
  }
  @Input() highlight?: boolean
  creditCard: BuyerCreditCard
  address: Address
}
