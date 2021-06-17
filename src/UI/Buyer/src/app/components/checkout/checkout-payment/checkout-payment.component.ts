import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  Input,
} from '@angular/core'
import {
  BuyerCreditCard,
  ListPage,
  OrderPromotion,
} from 'ordercloud-javascript-sdk'
import {
  HSOrder,
} from '@ordercloud/headstart-sdk'
import { FormGroup, FormControl } from '@angular/forms'
import { groupBy as _groupBy } from 'lodash'
import { faCheckCircle } from '@fortawesome/free-solid-svg-icons'
import { ToastrService } from 'ngx-toastr'
import { uniqBy as _uniqBy } from 'lodash'
import { CheckoutService } from 'src/app/services/order/checkout.service'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { SelectedCreditCard } from 'src/app/models/credit-card.types'
import { AcceptedPaymentTypes, IGroupedOrderPromo } from 'src/app/models/checkout.types'
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
  _orderPromos: OrderPromotion[]
  _uniqueOrderPromos: OrderPromotion[]
  _groupedOrderPromos: IGroupedOrderPromo
  _acceptedPaymentMethods: string[]
  promoForm: FormGroup
  promoCode = ''
  selectedPaymentMethod: AcceptedPaymentTypes
  POTermsAccepted: boolean
  faCheckCircle = faCheckCircle

  constructor(
    private context: ShopperContextService,
    private toastrService: ToastrService,
    private appConfig: AppConfig
  ) { }

  ngOnInit(): void {
    this._orderCurrency = this.context.currentUser.get().Currency
    this.setOrderPromos()
    this._acceptedPaymentMethods = this.appConfig.acceptedPaymentMethods
    console.log(this._acceptedPaymentMethods)
    this.selectedPaymentMethod = this._acceptedPaymentMethods?.[0] as AcceptedPaymentTypes
    this.createPromoForm(this.promoCode)
  }

  createPromoForm(promoCode: string): void {
    this.promoForm = new FormGroup({
      PromoCode: new FormControl(promoCode),
    })
  }

  updatePromoCodeValue(event: any): void {
    this.promoCode = event.target.value
  }

  selectPaymentMethod(e: any): void {
    this.selectedPaymentMethod = e.target.value
    console.log(this.selectedPaymentMethod)
  }

  getPaymentMethodName(method: string): string {
    return method.split(/(?=[A-Z])/).join(' ')
  }

  acceptPOTerms(): void {
    this.POTermsAccepted = true
  }

  async applyPromo(): Promise<void> {
    try {
      await this.context.order.promos.applyPromo(this.promoCode)
      this.setOrderPromos()
      await this.checkout.calculateOrder()
      this.promoCode = ''
    } catch (ex) {
      this.toastrService.error('Invalid or inelligible promotion.')
    } finally {
      this.promosChanged.emit(this._orderPromos)
    }
  }

  async removePromo(promoCode: string): Promise<void> {
    try {
      await this.context.order.promos.removePromo(promoCode)
      this.setOrderPromos()
      await this.checkout.calculateOrder()
    } finally {
      this.promosChanged.emit(this._orderPromos)
    }
  }

  getPromoDiscountTotal(promoID: string): number {
    return this._orderPromos
      .filter((promo) => promo.ID === promoID)
      .reduce((accumulator, promo) => promo.Amount + accumulator, 0)
  }

  onCardSelected(card: SelectedCreditCard): void {
    this.cardSelected.emit(card)
  }

  setOrderPromos(): void {
    this._orderPromos = this.context.order.promos.get().Items
    this._uniqueOrderPromos = _uniqBy(this._orderPromos, 'Code')
  }

  // used when no selection of card is required
  // only acknowledgement of purchase order is required
  onContinue(): void {
    this.continue.emit()
  }
}

