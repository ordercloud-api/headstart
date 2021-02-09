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
} from 'ordercloud-javascript-sdk'
import {
  HSOrder,
  HSLineItem,
  HSPayment,
  OrderCloudIntegrationsCreditCardPayment,
  HeadStartSDK,
} from '@ordercloud/headstart-sdk'
import {
  getOrderSummaryMeta,
} from 'src/app/services/purchase-order.helper'
import { NgxSpinnerService } from 'ngx-spinner'
import { ToastrService } from 'ngx-toastr'
import {
  extractMiddlewareError,
  isInventoryError,
  getPaymentError,
  ErrorCodes,
} from 'src/app/services/error-constants'
import { AxiosError } from 'axios'
import { CheckoutService } from 'src/app/services/order/checkout.service'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { CheckoutSection } from 'src/app/models/checkout.types'
import { HSBuyerCreditCard, SelectedCreditCard } from 'src/app/models/credit-card.types'
import { OrderSummaryMeta } from 'src/app/models/order.types'
import { ModalState } from 'src/app/models/shared.types'
import { MiddlewareError } from 'src/app/models/error.types'
import { Router } from '@angular/router'



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
  orderError: string
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
    private router: Router
  ) {}

  async ngOnInit(): Promise<void> {
    this.context.order.onChange((order) => (this.order = order))
    this.order = this.context.order.get()
    if (this.order.IsSubmitted) {
      await this.handleOrderError('This order has already been submitted')
    }

    this.lineItems = this.context.order.cart.get()
    this.orderPromotions = this.context.order.promos.get().Items
    this.isAnon = this.context.currentUser.isAnonymous()
    this.currentPanel = this.isAnon ? 'login' : 'shippingAddressLoading'
    this.initLoadingIndicator();
    this.orderSummaryMeta = getOrderSummaryMeta(
      this.order,
      this.orderPromotions,
      this.lineItems.Items,
      this.shipEstimates,
      this.currentPanel
    )
    this.setValidation('login', !this.isAnon)

    this.isNewCard = false
    this.invalidLineItems = await this.context.order.cart.getInvalidLineItems()
    if (this.invalidLineItems?.length) {
      // Navigate to cart to review invalid itemss
      void this.router.navigate(['/cart'])
    } else {
      await this.reIDLineItems()
      this.destoryLoadingIndicator('shippingAddress')
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
    this.cards = await this.context.currentUser.cards.List()
    await this.context.order.promos.applyAutomaticPromos()
    this.order = this.context.order.get()
    if (this.order.IsSubmitted) {
      await this.handleOrderError('This order has already been submitted')
    }
    this.lineItems = this.context.order.cart.get()
    this.destoryLoadingIndicator('payment')
  }

  navigateBackToAddress(): void {
    this.toSection('shippingAddress')
  }

  buildCCPayment(card: HSBuyerCreditCard): Payment {
    // amount gets calculated in middleware
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
      // need to figure out how to use the platform. ran into creditCardID cannot be null.
      // so for now I always save any credit card in OC.
      this.selectedCard.SavedCard = await this.context.currentUser.cards.Save(
        output.NewCard
      )
      this.isNewCard = true
      payments.push(this.buildCCPayment(this.selectedCard.SavedCard))
    } else {
      payments.push(this.buildCCPayment(output.SavedCard))
      delete this.selectedCard.NewCard
    }
    if (this.orderSummaryMeta.POLineItemCount) {
      payments.push(this.buildPOPayment())
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
      // Navigate to cart to review invalid itemss
      await this.context.order.reset() // orderID might've been incremented
      this.isLoading = false
      void this.router.navigate(['/cart'])
    } else {
      this.initLoadingIndicator('submitLoading')
      await this.checkout.addComment(comment)
      try {
        const payment = this.orderSummaryMeta.StandardLineItemCount
          ? this.getCCPaymentData()
          : {}
        const order = await HeadStartSDK.Orders.Submit(
          'Outgoing',
          this.order.ID,
          payment
        )
        await this.checkout.appendPaymentMethodToOrderXp(order.ID, payment)
        this.isLoading = false
        await this.context.order.reset() // get new current order
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

    const error = extractMiddlewareError(exception)
    if (!error) {
      throw new Error('An unknown error has occurred')
    }
    if (error.ErrorCode === ErrorCodes.OrderSubmit.AlreadySubmitted) {
      await this.handleOrderError('This order has already been submitted')
    } else if (
      error.ErrorCode === ErrorCodes.OrderSubmit.MissingShippingSelections
    ) {
      this.handleMissingShippingSelectionsError()
    } else if (
      error.ErrorCode === ErrorCodes.OrderSubmit.OrderCloudValidationError
    ) {
      this.handleOrderCloudValidationError(error)
    } else if (
      error.ErrorCode === ErrorCodes.Payment.FailedToVoidAuthorization
    ) {
      this.handleVoidAuthorizationError()
    } else if (error.ErrorCode?.includes('CreditCardAuth.')) {
      await this.handleCreditcardError(error)
    } else if (error.ErrorCode == ErrorCodes.InternalServerError) {
      throw new Error(error.Message)
    } else {
      throw new Error(error as any)
    }
  }

  async handleOrderError(message: string): Promise<void> {
    this.orderError = message
    await this.context.order.reset()
    this.orderErrorModal = ModalState.Open
  }

  handleMissingShippingSelectionsError(): void {
    this.currentPanel = 'shipping'
    throw new Error(
      'Order missing shipping selections - please select shipping selections'
    )
  }

  handleOrderCloudValidationError(error: MiddlewareError): void {
    // TODO: blow this out into a modal that allows user to easily take action on line items in cart
    const innerErrors = error.Data as MiddlewareError[]
    innerErrors.forEach((e) => {
      if (isInventoryError(e)) {
        if (e.ErrorCode === 'Inventory.Insufficient') {
          throw new Error(
            `Insufficient inventory for product ${e.Data.ProductID}. Quantity Available ${e.Data.QuantityAvailable}`
          )
        }
      }
    })
  }

  handleVoidAuthorizationError(): void {
    // We tried to void an authorization on an order and it failed, will require manual intervention from support
    // we email as well as log the error in application insights
    this.currentPanel = 'payment'
    this.paymentError =
      'Critical error occurred. Please contact support and avoid submitting order again until you have been helped'
  }

  async handleCreditcardError(error: MiddlewareError): Promise<void> {
    this.currentPanel = 'payment'
    this.paymentError = getPaymentError(error.Message)
    if (this.isNewCard) {
      await this.context.currentUser.cards.Delete(
        this.getCCPaymentData().CreditCardID
      )
    }
  }

  getCCPaymentData(): OrderCloudIntegrationsCreditCardPayment {
    return {
      OrderID: this.order.ID,
      PaymentID: this.payments.Items[0].ID, // There's always only one at this point
      CreditCardID: this.selectedCard?.SavedCard?.ID,
      CreditCardDetails: this.selectedCard.NewCard,
      Currency: this.order.xp.Currency,
      CVV: this.selectedCard.CVV,
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
    this.orderSummaryMeta = getOrderSummaryMeta(
      this.order,
      this.orderPromotions,
      this.lineItems.Items,
      this.shipEstimates,
      id
    )
    const prevIdx = Math.max(this.sections.findIndex((x) => x.id === id) - 1, 0)

    // set validation to true on all previous sections
    for (let i = 0; i <= prevIdx; i++) {
      const prev = this.sections[i].id
      this.setValidation(prev, true)
    }
    this.accordian.toggle(id)
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

  updateOrderMeta(promos?: CustomEvent<OrderPromotion[]>): void {
    this.orderPromotions = this.context.order.promos.get().Items
    this.orderPromotions = promos.detail
    this.orderSummaryMeta = getOrderSummaryMeta(
      this.order,
      this.orderPromotions,
      this.lineItems.Items,
      this.shipEstimates,
      this.currentPanel
    )
  }

  initLoadingIndicator(toSection?: string): void {
    this.isLoading = true
    if(toSection) {
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
