import { Component, ViewChild, OnInit } from '@angular/core'
import { faCheck } from '@fortawesome/free-solid-svg-icons'
import { NgbAccordion, NgbPanelChangeEvent } from '@ng-bootstrap/ng-bootstrap'
import {
  ShipMethodSelection,
  ShipEstimate,
  ListPage,
  Payment,
  BuyerCreditCard,
  OrderPromotion,
  Orders,
} from 'ordercloud-javascript-sdk'
import {
  HSOrder,
  HSLineItem,
  HSPayment,
  OrderCloudIntegrationsCreditCardPayment,
  HeadStartSDK,
  OrderCloudIntegrationsCreditCardToken,
} from '@ordercloud/headstart-sdk'
import { getOrderSummaryMeta } from 'src/app/services/purchase-order.helper'
import { NgxSpinnerService } from 'ngx-spinner'
import { ToastrService } from 'ngx-toastr'
import {
  extractMiddlewareError,
  isInventoryError,
  ErrorCodes,
} from 'src/app/services/error-constants'
import { AxiosError } from 'axios'
import { CheckoutService } from 'src/app/services/order/checkout.service'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import {
  AcceptedPaymentTypes,
  CheckoutSection,
} from 'src/app/models/checkout.types'
import {
  HSBuyerCreditCard,
  SelectedCreditCard,
} from 'src/app/models/credit-card.types'
import { OrderSummaryMeta } from 'src/app/models/order.types'
import { ModalState } from 'src/app/models/shared.types'
import { ErrorDisplayData, MiddlewareError } from 'src/app/models/error.types'
import { Router } from '@angular/router'
import { TranslateService } from '@ngx-translate/core'

@Component({
  templateUrl: './checkout.component.html',
  styleUrls: ['./checkout.component.scss'],
})
export class OCMCheckout implements OnInit {
  @ViewChild('acc', { static: false }) public accordian: NgbAccordion
  isAnon: boolean
  isNewCard: boolean
  order: HSOrder
  orderPromotions: OrderPromotion[] = []
  lineItems: ListPage<HSLineItem>
  invalidLineItems: HSLineItem[] = []
  orderSummaryMeta: OrderSummaryMeta
  payments: ListPage<Payment>
  cards: ListPage<BuyerCreditCard>
  selectedCard: SelectedCreditCard
  shipEstimates: ShipEstimate[] = []
  currentPanel: string
  paymentError: string
  orderErrorTitle: string
  orderErrorMessage: string
  orderErrorButtonText: string
  checkoutError: MiddlewareError
  faCheck = faCheck
  orderErrorModal = ModalState.Closed
  checkout: CheckoutService = this.context.order.checkout
  sections: CheckoutSection[] = [
    {
      id: 'login',
      valid: false,
    },
    {
      id: 'shippingAddress',
      valid: false,
    },
    {
      id: 'shippingSelection',
      valid: false,
    },
    {
      id: 'payment',
      valid: false,
    },
    {
      id: 'confirm',
      valid: false,
    },
  ]
  isLoading = false

  constructor(
    private context: ShopperContextService,
    private spinner: NgxSpinnerService,
    private toastrService: ToastrService,
    private router: Router,
    private translate: TranslateService
  ) {}

  async ngOnInit(): Promise<void> {
    this.context.order.onChange((order) => (this.order = order))
    this.order = this.context.order.get()
    if (this.order.IsSubmitted) {
      await this.handleOrderError(ErrorCodes.AlreadySubmitted)
    }

    this.lineItems = this.context.order.cart.get()
    this.orderPromotions = this.context.order.promos.get()?.Items
    this.isAnon = this.context.currentUser.isAnonymous()
    this.currentPanel = this.isAnon ? 'login' : 'shippingAddressLoading'
    this.initLoadingIndicator()
    this.updateOrderMeta()
    this.setValidation('login', !this.isAnon)
    await this.context.order.promos.applyAutomaticPromos()
    this.isNewCard = false
    this.invalidLineItems = await this.context.order.cart.getInvalidLineItems()
    if (this.invalidLineItems?.length) {
      // Navigate to cart to review invalid items
      void this.router.navigate(['/cart'])
    } else {
      await this.reIDLineItems()
      this.destoryLoadingIndicator(this.isAnon ? 'login' : 'shippingAddress')
    }
  }

  async reIDLineItems(): Promise<void> {
    await this.checkout.cleanLineItemIDs(this.order.ID, this.lineItems.Items)
    this.lineItems = this.context.order.cart.get()
  }

  async doneWithShipToAddress(): Promise<void> {
    this.initLoadingIndicator()
    const orderWorksheet = await this.checkout.estimateShipping()
    this.shipEstimates = orderWorksheet.ShipEstimateResponse.ShipEstimates
    this.destoryLoadingIndicator('shippingSelection')
  }

  async selectShipMethod(selection: ShipMethodSelection): Promise<void> {
    const orderWorksheet = await this.checkout.selectShipMethods([selection])
    this.shipEstimates = orderWorksheet.ShipEstimateResponse.ShipEstimates
  }

  async doneWithShippingRates(): Promise<void> {
    this.initLoadingIndicator('shippingSelectionLoading')
    await this.checkout.calculateOrder()
    this.cards = await this.context.currentUser.cards.List(this.isAnon)
    this.order = this.context.order.get()
    if (this.order.IsSubmitted) {
      await this.handleOrderError(ErrorCodes.AlreadySubmitted)
    }
    this.lineItems = this.context.order.cart.get()
    this.destoryLoadingIndicator('payment')
  }

  navigateBackToAddress(): void {
    this.toSection('shippingAddress')
  }

  buildCCPaymentFromNewCard(
    card: OrderCloudIntegrationsCreditCardToken
  ): Payment {
    return {
      DateCreated: new Date().toDateString(),
      Accepted: false,
      Type: 'CreditCard',
      xp: {
        partialAccountNumber: card.AccountNumber.substr(
          card.AccountNumber.length - 4
        ),
        cardType: card.CardType,
      },
    }
  }

  buildCCPaymentFromSavedCard(card: HSBuyerCreditCard): Payment {
    return {
      DateCreated: new Date().toDateString(),
      Accepted: false,
      Type: 'CreditCard',
      CreditCardID: card.ID,
      xp: {
        // any additions to the xp model must be updated in c# HSPaymentModel or won't show up
        partialAccountNumber: card.PartialAccountNumber,
        cardType: card.CardType,
      },
    }
  }

  buildPOPayment(): Payment {
    // amount gets calculated in middleware
    return {
      DateCreated: new Date().toDateString(),
      Type: 'PurchaseOrder',
    }
  }

  async onCardSelected(output: SelectedCreditCard): Promise<void> {
    this.initLoadingIndicator('paymentLoading')
    const payments: HSPayment[] = []
    this.selectedCard = output
    if (!output.SavedCard) {
      payments.push(await this.handleNewCard(output))
    } else {
      payments.push(this.buildCCPaymentFromSavedCard(output.SavedCard))
      delete this.selectedCard.NewCard
    }
    try {
      await HeadStartSDK.Payments.SavePayments(this.order.ID, {
        Payments: payments,
      })
      this.payments = await this.checkout.listPayments()
      this.destoryLoadingIndicator('confirm')
    } catch (exception) {
      this.setValidation('payment', false)
      await this.handleSubmitError(exception)
    }
  }

  async handleNewCard(output: SelectedCreditCard): Promise<HSPayment> {
    this.isNewCard = true
    if (this.isAnon) {
      await this.context.order.checkout.setOneTimeAddress(
        output.NewCard.CCBillingAddress,
        'billing'
      )
      return this.buildCCPaymentFromNewCard(output.NewCard)
    } else {
      this.selectedCard.SavedCard = await this.context.currentUser.cards.Save(
        output.NewCard
      )
      return this.buildCCPaymentFromSavedCard(this.selectedCard.SavedCard)
    }
  }

  async onAcknowledgePurchaseOrder(): Promise<void> {
    //  Function that is used when there are no credit cards. Just PO acknowledgement
    const payments = [this.buildPOPayment()]
    await HeadStartSDK.Payments.SavePayments(this.order.ID, {
      Payments: payments,
    })
    this.payments = await this.checkout.listPayments()
    this.toSection('confirm')
  }

  async submitOrderWithComment(comment: string): Promise<void> {
    // Check that line items in cart are all from active products (none were made inactive during checkout).
    this.invalidLineItems = await this.context.order.cart.getInvalidLineItems()
    if (this.invalidLineItems?.length) {
      // Navigate to cart to review invalid items
      await this.context.order.reset() // orderID might've been incremented
      this.isLoading = false
      void this.router.navigate(['/cart'])
    } else {
      this.initLoadingIndicator('submitLoading')
      try {
        const payment =
          this.payments?.Items?.[0]?.Type === AcceptedPaymentTypes.CreditCard
            ? this.getCCPaymentData()
            : {}
        const order = await HeadStartSDK.Orders.Submit(
          'Outgoing',
          this.order.ID,
          payment
        )
        //  Do all patching of order XP values in the OrderSubmit integration event
        //  Patching order XP before order is submitted will clear out order worksheet data
        await this.checkout.patch({ Comments: comment }, order.ID)
        await this.context.order.reset() // get new current order
        this.isLoading = false
        this.toastrService.success('Order submitted successfully', 'Success')
        this.context.router.toMyOrderDetails(order.ID)
      } catch (e) {
        await this.handleSubmitError(e)
      }
    }
  }

  async handleSubmitError(exception: AxiosError): Promise<void> {
    await this.context.order.reset() // orderID might've been incremented
    this.isLoading = false
    this.checkoutError = extractMiddlewareError(exception)
    if (
      this.checkoutError.ErrorCode === ErrorCodes.OrderCloudValidationError.code
    ) {
      this.handleOrderCloudValidationError(this.checkoutError)
    } else if (!this.checkoutError || !this.checkoutError.ErrorCode) {
      throw new Error(this.translate.instant('ERRORS.UNKNOWN'))
    } else if (
      this.checkoutError.ErrorCode === ErrorCodes.InternalServerError.code
    ) {
      throw new Error(this.checkoutError.Message)
    } else if (this.checkoutError.ErrorCode?.includes('CreditCardAuth.')) {
      await this.handleCreditcardError(this.checkoutError)
    } else {
      const matchingError = this.getMatchingError(this.checkoutError)
      if (matchingError) {
        this.handleOrderError(matchingError)
      } else {
        throw new Error(this.checkoutError as any)
      }
    }
  }

  getMatchingError(error: MiddlewareError): ErrorDisplayData {
    const matchingError = Object.keys(ErrorCodes).find((key) => {
      return ErrorCodes[key].code === error.ErrorCode
    })
    if (matchingError) {
      return ErrorCodes[matchingError]
    }
  }

  handleOrderError(error: ErrorDisplayData, customMessage?: string): void {
    this.orderErrorTitle = this.translate.instant(error.title)
    // custom message would already be translated
    this.orderErrorMessage = customMessage
      ? customMessage
      : this.translate.instant(error.message)
    this.orderErrorButtonText = this.translate.instant(error.buttonText)
    this.orderErrorModal = ModalState.Open
  }

  handleOrderCloudValidationError(error: MiddlewareError): void {
    // TODO: blow this out into a modal that allows user to easily take action on line items in cart
    const innerErrors = error.Data as MiddlewareError[]
    const inventoryError = innerErrors.find((e) => isInventoryError(e))
    if (inventoryError) {
      const part1 = this.translate.instant('ERRORS.INSUFFICIENT.MESSAGE_PART1')
      const part2 = this.translate.instant('ERRORS.INSUFFICIENT.MESSAGE_PART2')
      const customMessage = `${part1} ${inventoryError.Data.ProductID}. ${part2}`
      this.handleOrderError(ErrorCodes.Insufficient, customMessage)
    } else {
      throw new Error(error as any)
    }
  }

  getPaymentError(errorReason: string): string {
    const reason = errorReason.replace('AVS', 'Address Verification') // AVS isn't likely something to be understood by a layperson
    const part1 = this.translate.instant(
      'ERRORS.CREDIT_CARD_AUTH.MESSAGE_PART1'
    )
    const part2 = this.translate.instant(
      'ERRORS.CREDIT_CARD_AUTH.MESSAGE_PART2'
    )
    return `${part1} "${reason}". ${part2}`
  }

  async handleCreditcardError(error: MiddlewareError): Promise<void> {
    const matchingError = ErrorCodes.CreditCardAuth
    this.paymentError = this.getPaymentError(error.Message)
    this.handleOrderError(matchingError, this.paymentError)
    if (this.isNewCard) {
      await this.context.currentUser.cards.Delete(
        this.getCCPaymentData().CreditCardID
      )
    }
  }

  handleAddressDismissal(): void {
    this.toSection('login')
  }

  async handleCheckoutError(): Promise<void> {
    this.orderErrorModal = ModalState.Closed
    await this.refreshOrderUpdateMeta()
    if (
      this.checkoutError.ErrorCode === ErrorCodes.MissingShippingSelections.code
    ) {
      this.currentPanel = 'shippingAddress'
    } else if (
      this.checkoutError.ErrorCode === ErrorCodes.AlreadySubmitted.code
    ) {
      this.router.navigateByUrl('/home')
    } else if (
      this.checkoutError.ErrorCode ===
        ErrorCodes.FailedToVoidAuthorization.code ||
      this.checkoutError.ErrorCode?.includes('CreditCardAuth.')
    ) {
      this.currentPanel = 'payment'
    } else if (
      this.checkoutError.ErrorCode === ErrorCodes.OrderCloudValidationError.code
    ) {
      this.router.navigateByUrl('/cart')
    }
    this.checkoutError = null
    this.orderErrorTitle = null
    this.orderErrorMessage = null
    this.orderErrorButtonText = null
  }

  getCCPaymentData(): OrderCloudIntegrationsCreditCardPayment {
    return {
      OrderID: this.order.ID,
      PaymentID: this.payments.Items[0].ID, // There's always only one at this point
      CreditCardID: this.selectedCard?.SavedCard?.ID,
      CreditCardDetails: this.selectedCard?.NewCard,
      Currency: this.order.xp.Currency,
      CVV: this.selectedCard?.CVV,
    }
  }

  getValidation(id: string): any {
    return this.sections.find((x) => x.id === id).valid
  }

  setValidation(id: string, value: boolean): void {
    this.sections.find((x) => x.id === id).valid = value
  }

  toSection(id: string): void {
    this.orderPromotions = this.context.order.promos.get()?.Items
    this.updateOrderMeta(id)
    const prevIdx = Math.max(this.sections.findIndex((x) => x.id === id) - 1, 0)
    // set validation to true on all previous sections
    for (let i = 0; i <= prevIdx; i++) {
      const prev = this.sections[i].id
      if (prev !== id) {
        this.setValidation(prev, true)
      }
    }
    this.accordian?.toggle(id)
  }

  beforeChange($event: NgbPanelChangeEvent): void {
    if (this.currentPanel === $event.panelId) {
      return $event.preventDefault()
    }

    // Only allow a section to open if all previous sections are valid
    for (const section of this.sections) {
      if (section.id === $event.panelId) {
        break
      }

      if (!section.valid) {
        return $event.preventDefault()
      }
    }
    this.currentPanel = $event.panelId
  }

  async refreshOrderUpdateMeta(): Promise<void> {
    await this.context.order.reset()
    this.lineItems = this.context.order.cart.get()
    this.orderPromotions = this.context.order.promos.get()?.Items
    this.updateOrderMeta()
  }

  updateOrderMeta(panelID?: string): void {
    this.orderSummaryMeta = getOrderSummaryMeta(
      this.order,
      this.orderPromotions,
      this.lineItems.Items,
      panelID || this.currentPanel
    )
  }

  initLoadingIndicator(toSection?: string): void {
    this.isLoading = true
    if (toSection) {
      this.currentPanel = toSection
    }
    this.spinner.show()
  }

  destoryLoadingIndicator(toSection: string): void {
    this.isLoading = false
    this.currentPanel = toSection
    this.toSection(toSection)
  }
}
