import { Component, Output, EventEmitter, OnInit, Input } from '@angular/core'
import {
  BuyerCreditCard,
  ListPage,
  OrderPromotion,
} from 'ordercloud-javascript-sdk'
import { HSOrder } from '@ordercloud/headstart-sdk'
import { groupBy as _groupBy } from 'lodash'
import { uniqBy as _uniqBy } from 'lodash'
import { CheckoutService } from 'src/app/services/order/checkout.service'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { SelectedCreditCard } from 'src/app/models/credit-card.types'
import { AcceptedPaymentTypes } from 'src/app/models/checkout.types'
import { OrderSummaryMeta } from 'src/app/models/order.types'
import { AppConfig } from 'src/app/models/environment.types'

@Component({
  templateUrl: './checkout-payment.component.html',
  styleUrls: ['./checkout-payment.component.scss'],
})
export class OCMCheckoutPayment implements OnInit {
  @Input() cards: ListPage<BuyerCreditCard>
  @Input() isAnon: boolean
  @Input() order: HSOrder
  @Input() paymentError: string
  @Input() orderSummaryMeta: OrderSummaryMeta
  @Output() cardSelected = new EventEmitter<SelectedCreditCard>()
  @Output() continue = new EventEmitter<void>()
  @Output() promosChanged = new EventEmitter<OrderPromotion[]>()
  checkout: CheckoutService = this.context.order.checkout
  _orderCurrency: string
  _acceptedPaymentMethods: string[]
  selectedPaymentMethod: AcceptedPaymentTypes
  POTermsAccepted: boolean

  constructor(
    private context: ShopperContextService,
    private appConfig: AppConfig
  ) {}

  ngOnInit(): void {
    this._orderCurrency = this.context.currentUser.get().Currency
    this._acceptedPaymentMethods = this.getAcceptedPaymentMethods()
    this.selectedPaymentMethod = this
      ._acceptedPaymentMethods?.[0] as AcceptedPaymentTypes
  }

  getAcceptedPaymentMethods(): string[] {
    if (
      this.appConfig?.acceptedPaymentMethods == null ||
      this.appConfig?.acceptedPaymentMethods?.length < 1
    ) {
      return [AcceptedPaymentTypes.CreditCard]
    }
    return this.appConfig.acceptedPaymentMethods
  }

  selectPaymentMethod(e: any): void {
    this.selectedPaymentMethod = e.target.value
  }

  getPaymentMethodName(method: string): string {
    return method.split(/(?=[A-Z])/).join(' ')
  }

  acceptPOTerms(): void {
    this.POTermsAccepted = true
  }

  onCardSelected(card: SelectedCreditCard): void {
    this.cardSelected.emit(card)
  }

  // used when no selection of card is required
  // only acknowledgement of purchase order is required
  onContinue(): void {
    this.continue.emit()
  }
}
