import { Component, Input, OnChanges } from '@angular/core'
import { faCcDiscover, faCcMastercard, faCcVisa } from '@fortawesome/free-brands-svg-icons'
import {
  faCreditCard,
  IconDefinition,
} from '@fortawesome/free-solid-svg-icons'

@Component({
  templateUrl: './credit-card-icon.component.html',
  styleUrls: ['./credit-card-icon.component.scss'],
})
export class OCMCreditCardIcon implements OnChanges {
  cardIcon: IconDefinition = faCreditCard
  visaIcon = faCcVisa
  mastercardIcon = faCcMastercard
  discoverIcon = faCcDiscover
  @Input() cardType: string
  @Input() size: string

  ngOnChanges(): void {
    if (!this.cardType) return
    this.cardIcon = this.setCardIcon(this.cardType)
  }

  setCardIcon(cardType: string): IconDefinition {
    switch (cardType.toLowerCase()) {
      case 'visa':
        return this.visaIcon
      case 'mastercard':
        return this.mastercardIcon
      case 'discover':
        return this.discoverIcon
    }
  }
}
