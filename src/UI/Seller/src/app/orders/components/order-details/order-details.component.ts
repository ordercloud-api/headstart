import { Component, Input, Inject } from '@angular/core'
import { OrderService } from '@app-seller/orders/order.service'
import {
  Address,
  LineItems,
  Payments,
  Payment,
  Orders,
  OrderDirection,
  Addresses,
  OrderReturns,
} from 'ordercloud-javascript-sdk'
import { PDFService } from '@app-seller/orders/pdf-render.service'
import {
  faDownload,
  faUndo,
  faExclamationTriangle,
  faInfoCircle,
  faUserAlt,
} from '@fortawesome/free-solid-svg-icons'
import { MiddlewareAPIService } from '@app-seller/shared/services/middleware-api/middleware-api.service'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import {
  HSLineItem,
  HSOrder,
  HeadStartSDK,
  HSOrderReturn,
  HSPayment,
} from '@ordercloud/headstart-sdk'
import { flatten as _flatten } from 'lodash'
import { OrderProgress, OrderType } from '@app-seller/models/order.types'
import { SELLER } from '@app-seller/models/user.types'
import { SupportedRates } from '@app-seller/shared'
import { UntypedFormControl, UntypedFormGroup } from '@angular/forms'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { CanReturnOrder } from '@app-seller/orders/line-item-status.helper'
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap'
import { OrderReturnCreateModal } from '../order-return-create-modal/order-return-create-modal.component'
import { Router } from '@angular/router'

export type LineItemTableValue = 'Default' | 'Backordered'

interface ILineItemTableStatus {
  [key: string]: LineItemTableValue
}

export const LineItemTableStatus: ILineItemTableStatus = {
  Default: 'Default',
  Backordered: 'Backordered',
}

@Component({
  selector: 'app-order-details',
  templateUrl: './order-details.component.html',
  styleUrls: ['./order-details.component.scss'],
})
export class OrderDetailsComponent {
  faDownload = faDownload
  faUndo = faUndo
  faExclamationTriangle = faExclamationTriangle
  faInfoCircle = faInfoCircle
  faUser = faUserAlt
  _order: HSOrder = {}
  _buyerOrder: HSOrder = {}
  _buyerQuoteAddress: Address = null
  _supplierOrder: HSOrder = {}
  _lineItems: HSLineItem[] = []
  _payments: Payment[] = []
  orderDirection: OrderDirection
  cardType: string
  createShipment: boolean
  exchangeRates: SupportedRates[]
  supplierCurrency: SupportedRates
  quotePricingForm: UntypedFormGroup
  isSellerUser = false
  isSaving = false
  isSettingQuotePrice = false
  quotedPrice = 0
  orderProgress: OrderProgress = {
    StatusDisplay: 'Processing',
    Value: 25,
    ProgressBarType: 'primary',
    Striped: false,
    Animated: false,
  }
  orderAvatarInitials: string
  orderReturns: HSOrderReturn[] = []
  modalReference: NgbModalRef

  @Input()
  set order(order: HSOrder) {
    if (Object.keys(order).length) {
      this.createShipment = false
      this.handleSelectedOrderChange(order)
    }
  }
  constructor(
    private orderService: OrderService,
    private pdfService: PDFService,
    private middleware: MiddlewareAPIService,
    private appAuthService: AppAuthService,
    private currentUserService: CurrentUserService,
    private modalService: NgbModal,
    private router: Router
  ) {
    this.isSellerUser = this.appAuthService.getOrdercloudUserType() === SELLER
    this.setOrderDirection()
  }

  async openReturnCreateModal(): Promise<void> {
    try {
      this.modalReference = this.modalService.open(OrderReturnCreateModal, {
        size: 'xl',
      })
      const componentInstance = this.modalReference
        .componentInstance as OrderReturnCreateModal
      componentInstance.order = this._order
      componentInstance.lineItems = this._lineItems
      componentInstance.orderReturns = this.orderReturns

      const orderReturnId = (await this.modalReference.result) as string

      this.router.navigateByUrl(`order-returns/${orderReturnId}`)
    } catch {
      // modal was dismissed, this can be safely ignored
    }
  }

  setOrderProgress(order: HSOrder): void {
    switch (order?.xp?.ShippingStatus) {
      case 'Processing':
        this.orderProgress = {
          StatusDisplay: 'Processing',
          Value: 25,
          ProgressBarType: 'primary',
          Striped: false,
          Animated: false,
        }
        break
      case 'PartiallyShipped':
        this.orderProgress = {
          StatusDisplay: 'Partially Shipped',
          Value: 50,
          ProgressBarType: 'primary',
          Striped: false,
          Animated: false,
        }
        break
      case 'Backordered':
        this.orderProgress = {
          StatusDisplay: 'Item Backordered',
          Value: 75,
          ProgressBarType: 'danger',
          Striped: true,
          Animated: true,
        }
        break
      case 'Shipped':
        this.orderProgress = {
          StatusDisplay: 'Complete',
          Value: 100,
          ProgressBarType: 'success',
          Striped: false,
          Animated: false,
        }
        break
    }
    if (order?.xp?.SubmittedOrderStatus === 'Canceled') {
      this.orderProgress = {
        StatusDisplay: 'Canceled',
        Value: 100,
        ProgressBarType: 'danger',
        Striped: false,
        Animated: false,
      }
    }
  }

  setCardType(payment: HSPayment): string {
    if (!payment.xp.cardType || payment.xp.cardType === null) {
      return 'Card'
    }
    this.cardType =
      payment.xp.cardType.charAt(0).toUpperCase() + payment.xp.cardType.slice(1)
    return this.cardType
  }

  getFullName(address: Address): string {
    const fullName = `${address?.FirstName || ''} ${address?.LastName || ''}`
    return fullName.trim()
  }

  setOrderDirection(): void {
    const url = window.location.href
    if (url.includes('Outgoing')) {
      this.orderDirection = 'Outgoing'
    } else {
      this.orderDirection = 'Incoming'
    }
  }

  async setOrderStatus(): Promise<void> {
    await this.middleware
      .acknowledgeQuoteOrder(this._order.ID)
      .then((completedOrder) => this.handleSelectedOrderChange(completedOrder))
  }

  isQuoteOrder(order: HSOrder): boolean {
    return this.orderService.isQuoteOrder(order)
  }

  isSupplierOrder(orderID: string): boolean {
    return this.orderService.isSupplierOrder(orderID)
  }

  setQuotePricingForm(): void {
    this.quotePricingForm = new UntypedFormGroup({
      QuotePrice: new UntypedFormControl(this._lineItems[0]?.UnitPrice),
    })
  }

  async overrideQuoteUnitPrice(): Promise<void> {
    await HeadStartSDK.Orders.OverrideQuoteUnitPrice(
      this._order.ID,
      this._lineItems[0].ID,
      this.quotePricingForm.value.QuotePrice
    )
    this.quotedPrice = 0 // Initialize, will be set during refreshOrder()
    await this.refreshOrder()
    this.isSettingQuotePrice = false
  }

  getOrderDate(): string {
    return this.isQuoteOrder(this._order)
      ? this._order.xp?.QuoteSubmittedDate
      : this._order.DateSubmitted
  }

  getQuotePriceButtonText(): string {
    return this.isSettingQuotePrice
      ? 'ADMIN.ORDERS.CANCEL_PRICING'
      : 'ADMIN.ORDERS.SET_QUOTE_PRICE'
  }

  toggleSetQuotePrice(): void {
    this.isSettingQuotePrice = !this.isSettingQuotePrice
    this.setQuotePricingForm()
  }

  async setData(order: HSOrder): Promise<void> {
    this._buyerQuoteAddress = null
    this._order = order
    this.exchangeRates = (await HeadStartSDK.ExchangeRates.GetRateList()).Items
    this.supplierCurrency = this.exchangeRates?.find(
      (r) => r.Currency === order.xp?.Currency
    )
    const currentUser = await this.currentUserService.getUser()
    if (!currentUser.Supplier) {
      // currently returns can only be viewed by admins
      const orderReturnsList = await OrderReturns.List({
        filters: { OrderID: order.ID },
      })
      this.orderReturns = orderReturnsList.Items
    }
    if (this.isSupplierOrder(order.ID) || this.isQuoteOrder(order)) {
      const orderData = await HeadStartSDK.Suppliers.GetSupplierOrder(
        order.ID,
        this._order.xp?.OrderType
      )
      this._buyerOrder = orderData.BuyerOrder.Order
      this._supplierOrder = orderData.SupplierOrder.Order
      this._lineItems = orderData.SupplierOrder.LineItems
    } else {
      this._buyerOrder = order
      this._lineItems = await this.getAllLineItems(order)
    }
    if (this.isQuoteOrder(order)) {
      if (order?.xp?.QuoteStatus === 'NeedsBuyerReview') {
        this.quotedPrice = this._lineItems[0]?.UnitPrice
      }
      const buyerId = this._buyerOrder.FromCompanyID
      if (this._buyerOrder.ShippingAddressID) {
        const address = await Addresses.Get(
          buyerId,
          this._buyerOrder.ShippingAddressID
        )
        this._buyerQuoteAddress = address
      }
    }
  }

  canCreateReturn(): boolean {
    return (
      this.isSellerUser && CanReturnOrder(this._lineItems, this.orderReturns)
    )
  }

  private async handleSelectedOrderChange(order: HSOrder): Promise<void> {
    this.setOrderDirection()
    await this.setData(order)
    this.orderAvatarInitials = !this.isQuoteOrder(order)
      ? `${this._buyerOrder?.FromUser?.FirstName?.slice(
          0,
          1
        ).toUpperCase()}${this._buyerOrder?.FromUser?.LastName?.slice(
          0,
          1
        ).toUpperCase()}`
      : `${order?.xp?.QuoteOrderInfo?.FirstName?.slice(
          0,
          1
        ).toUpperCase()}${order?.xp?.QuoteOrderInfo?.LastName?.slice(
          0,
          1
        ).toUpperCase()}`
    this.setOrderProgress(order)
    if (this._order?.xp?.OrderType != OrderType.Quote) {
      const paymentsResponse = await Payments.List(
        this.orderDirection,
        order.ID
      )
      this._payments = paymentsResponse.Items
    }
  }

  private async getAllLineItems(order): Promise<HSLineItem[]> {
    let lineItems = []
    const listOptions = {
      page: 1,
      pageSize: 100,
    }
    const lineItemsResponse = await LineItems.List(
      this.orderDirection,
      order.ID,
      listOptions
    )
    lineItems = [...lineItems, ...(lineItemsResponse.Items as HSLineItem[])]
    if (lineItemsResponse.Meta.TotalPages <= 1) {
      return lineItems as HSLineItem[]
    } else {
      let lineItemRequests = []
      for (let page = 2; page <= lineItemsResponse.Meta.TotalPages; page++) {
        listOptions.page = page
        lineItemRequests = [
          ...lineItemRequests,
          LineItems.List(this.orderDirection, order.ID, listOptions),
        ]
      }
      const response = await Promise.all(lineItemRequests).then((response) => {
        lineItems = [...lineItems, ..._flatten(response.map((r) => r.Items))]
        return lineItems
      })
      return response as HSLineItem[]
    }
  }

  async refreshOrder(): Promise<void> {
    let order: HSOrder
    if (this._order?.xp?.OrderType === OrderType.Quote) {
      order = await HeadStartSDK.Orders.GetQuoteOrder(this._order.ID)
    } else {
      order = await Orders.Get(this.orderDirection, this._order.ID)
    }
    this.handleSelectedOrderChange(order)
  }

  toggleCreateShipment(createShipment: boolean): void {
    this.createShipment = createShipment
  }

  protected createAndSavePDF(): void {
    this.pdfService.createAndSavePDF(this._order.ID)
  }

  buildOrderDetailsRoute(orderReturn: HSOrderReturn): string {
    return `/order-returns/${orderReturn.ID}`
  }
}
