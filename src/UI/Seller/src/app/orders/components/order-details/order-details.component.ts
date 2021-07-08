import { Component, Input, Inject } from '@angular/core'
import { OrderService } from '@app-seller/orders/order.service'
import {
  Address,
  OcLineItemService,
  OcPaymentService,
  Order,
  Payment,
  OcOrderService,
  OrderDirection,
  OcAddressService,
} from '@ordercloud/angular-sdk'

// temporarily any with sdk update
// import { ProductImage } from '@ordercloud/headstart-sdk';
import { PDFService } from '@app-seller/orders/pdf-render.service'
import {
  faDownload,
  faUndo,
  faExclamationTriangle,
  faInfoCircle,
  faUserAlt,
} from '@fortawesome/free-solid-svg-icons'
import { MiddlewareAPIService } from '@app-seller/shared/services/middleware-api/middleware-api.service'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { ReturnReason } from '@app-seller/shared/models/return-reason.interface'
import { HSLineItem, HSOrder, RMA } from '@ordercloud/headstart-sdk'
import { flatten as _flatten } from 'lodash'
import { OrderProgress } from '@app-seller/models/order.types'
import { AppConfig } from '@app-seller/models/environment.types'
import { SELLER } from '@app-seller/models/user.types'
import { RMAService } from '@app-seller/rmas/rmas.service'

export type LineItemTableValue =
  | 'Default'
  | 'Canceled'
  | 'Returned'
  | 'Backordered'

interface ILineItemTableStatus {
  [key: string]: LineItemTableValue
}

export const LineItemTableStatus: ILineItemTableStatus = {
  Default: 'Default',
  Canceled: 'Canceled',
  Returned: 'Returned',
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
  _order: Order = {}
  _buyerOrder: Order = {}
  _buyerQuoteAddress: Address = null
  _supplierOrder: Order = {}
  _lineItems: HSLineItem[] = []
  _payments: Payment[] = []
  images: any[] = []
  orderDirection: OrderDirection
  cardType: string
  createShipment: boolean
  isSellerUser = false
  isSaving = false
  orderProgress: OrderProgress = {
    StatusDisplay: 'Processing',
    Value: 25,
    ProgressBarType: 'primary',
    Striped: false,
    Animated: false,
  }
  orderAvatarInitials: string
  rmas: RMA[]

  @Input()
  set order(order: Order) {
    if (Object.keys(order).length) {
      this.createShipment = false
      this.handleSelectedOrderChange(order)
    }
  }
  constructor(
    private ocLineItemService: OcLineItemService,
    private ocOrderService: OcOrderService,
    private ocPaymentService: OcPaymentService,
    private orderService: OrderService,
    private pdfService: PDFService,
    private middleware: MiddlewareAPIService,
    private appAuthService: AppAuthService,
    private ocAddressService: OcAddressService,
    private rmaService: RMAService,
    @Inject(applicationConfiguration) private appConfig: AppConfig
  ) {
    this.isSellerUser = this.appAuthService.getOrdercloudUserType() === SELLER
    this.setOrderDirection()
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
    if (order?.xp?.ClaimStatus === 'Pending') {
      this.orderProgress = {
        StatusDisplay: 'Needs Attention',
        Value: 100,
        ProgressBarType: 'danger',
        Striped: true,
        Animated: true,
      }
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

  setCardType(payment): string {
    if (!payment.xp.cardType || payment.xp.cardType === null) {
      return 'Card'
    }
    this.cardType =
      payment.xp.cardType.charAt(0).toUpperCase() + payment.xp.cardType.slice(1)
    return this.cardType
  }

  getReturnReason(reasonCode: string): string {
    return ReturnReason[reasonCode]
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

  isQuoteOrder(order: Order): boolean {
    return this.orderService.isQuoteOrder(order)
  }

  isSupplierOrder(orderID: string): boolean {
    return this.orderService.isSupplierOrder(orderID)
  }

  async setData(order: Order): Promise<void> {
    this._buyerQuoteAddress = null
    this._order = order
    const rmaListPage = await this.rmaService.listRMAsByOrderID(order.ID)
    this.rmas = rmaListPage.Items
    if (this.isSupplierOrder(order.ID)) {
      const orderData = await this.middleware.getSupplierData(order.ID)
      this._buyerOrder = orderData.BuyerOrder.Order
      this._supplierOrder = orderData.SupplierOrder.Order
      this._lineItems = orderData.SupplierOrder.LineItems
    } else {
      this._buyerOrder = order
      this._lineItems = await this.getAllLineItems(order)
    }
    if (this.isQuoteOrder(order)) {
      const buyerId = this._buyerOrder.FromCompanyID
      if (this._buyerOrder.ShippingAddressID) {
        const address = await this.ocAddressService
          .Get(buyerId, this._buyerOrder.ShippingAddressID)
          .toPromise()
        this._buyerQuoteAddress = address
      }
    }
  }

  private async handleSelectedOrderChange(order: Order): Promise<void> {
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
    const paymentsResponse = await this.ocPaymentService
      .List(this.orderDirection, order.ID)
      .toPromise()
    this._payments = paymentsResponse.Items
  }

  private async getAllLineItems(order) {
    let lineItems = []
    const listOptions = {
      page: 1,
      pageSize: 100,
    }
    const lineItemsResponse = await this.ocLineItemService
      .List(this.orderDirection, order.ID, listOptions)
      .toPromise()
    lineItems = [...lineItems, ...(lineItemsResponse.Items as HSLineItem[])]
    if (lineItemsResponse.Meta.TotalPages <= 1) {
      return lineItems
    } else {
      let lineItemRequests = []
      for (let page = 2; page <= lineItemsResponse.Meta.TotalPages; page++) {
        listOptions.page = page
        lineItemRequests = [
          ...lineItemRequests,
          this.ocLineItemService
            .List(this.orderDirection, order.ID, listOptions)
            .toPromise(),
        ]
      }
      return await Promise.all(lineItemRequests).then((response) => {
        lineItems = [...lineItems, ..._flatten(response.map((r) => r.Items))]
        return lineItems
      })
    }
  }

  async refreshOrder(): Promise<void> {
    const order = await this.ocOrderService
      .Get(this.orderDirection, this._order.ID)
      .toPromise()
    this.handleSelectedOrderChange(order)
  }

  toggleCreateShipment(createShipment: boolean): void {
    this.createShipment = createShipment
  }

  protected createAndSavePDF(): void {
    this.pdfService.createAndSavePDF(this._order.ID)
  }

  buildOrderDetailsRoute(rma: RMA): string {
    return `/rmas/${rma.RMANumber}`
  }
}
