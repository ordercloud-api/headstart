import { Component, OnInit, ViewChild } from '@angular/core'
import {
  faCube,
  faExchangeAlt,
  faTruck,
} from '@fortawesome/free-solid-svg-icons'
import {
  HSOrder,
  OrderDetails,
  HSLineItem,
  HeadStartSDK,
} from '@ordercloud/headstart-sdk'
import { NgbModal, NgbNav } from '@ng-bootstrap/ng-bootstrap'
import { isQuoteOrder } from '../../../services/orderType.helper'
import { CanReturnOrder } from 'src/app/services/lineitem-status.helper'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import {
  OrderReorderResponse,
  OrderViewContext,
} from 'src/app/models/order.types'
import { ToastrService } from 'ngx-toastr'

type OrderDetailsTab = 'details' | 'shipments' | 'orderreturns'
@Component({
  templateUrl: './order-detail.component.html',
  styleUrls: ['./order-detail.component.scss'],
})
export class OCMOrderDetails implements OnInit {
  order: HSOrder
  orderDetails: OrderDetails
  approvalVersion = false
  faCube = faCube
  faTruck = faTruck
  faExchangeAlt = faExchangeAlt
  activeTab = 0
  reorderResponse: OrderReorderResponse
  message = { string: null, classType: null }
  showRequestReturn = false
  isAnon: boolean
  isQuoteOrder = isQuoteOrder
  @ViewChild('nav') orderDetailsNav: NgbNav

  constructor(
    private context: ShopperContextService,
    private modalService: NgbModal,
    private toastrService: ToastrService
  ) {}

  async ngOnInit(): Promise<void> {
    this.isAnon = this.context.currentUser.isAnonymous()
    this.approvalVersion =
      this.context.orderHistory.filters.getOrderViewContext() ===
      OrderViewContext.Approve
    this.orderDetails = await this.context.orderHistory.getOrderDetails()
    this.order = this.orderDetails.Order
    this.validateReorder(this.order.ID, this.orderDetails.LineItems)
  }

  open(content: HTMLTemplateElement): void {
    if (this.reorderResponse) {
      this.modalService.open(content, { ariaLabelledBy: 'confirm-modal' })
    }
  }

  async validateReorder(
    orderID: string,
    lineItems: HSLineItem[]
  ): Promise<void> {
    this.reorderResponse = await this.context.orderHistory.validateReorder(
      orderID,
      lineItems
    )
    this.updateMessage(this.reorderResponse)
  }

  isFavorite(orderID: string): boolean {
    return this.context.currentUser.get().FavoriteOrderIDs.includes(orderID)
  }

  getOrderStatus(): string {
    // AwaitingApproval is the one status order xp doesn't account for. If order.status is AwaitingApproval, take that.
    if (this.order?.Status === 'AwaitingApproval') {
      return 'AwaitingApproval'
    } else if (this.order?.xp?.OrderType === 'Quote') {
      return this.order?.xp?.QuoteStatus
    } else {
      return this.order?.xp?.SubmittedOrderStatus
    }
  }

  async cancelOrder(): Promise<void> {
    await HeadStartSDK.Orders.Cancel(this.order.ID)
    this.toastrService.success('Order cancelled')
    this.ngOnInit()
  }

  canRequestReturn(): boolean {
    return CanReturnOrder(
      this.orderDetails.LineItems,
      this.orderDetails.OrderReturns
    )
  }

  canCancelOrder(): boolean {
    return (
      !this.isQuoteOrder(this.order) &&
      this.orderDetails.Order.Status === 'Open' &&
      !this.orderDetails.LineItems.some((li) => li.QuantityShipped > 0)
    )
  }

  toggleFavorite(order: HSOrder): void {
    const newValue = !this.isFavorite(order.ID)
    this.context.currentUser.setIsFavoriteOrder(newValue, order.ID)
  }

  toggleRequestReturn(): void {
    this.showRequestReturn = !this.showRequestReturn
  }

  toAllOrders(): void {
    this.context.router.toMyOrders()
  }

  openNavItem(tab: OrderDetailsTab): void {
    const tabMap = {
      details: 0,
      shipments: 1,
      orderreturns: 2,
    }
    this.orderDetailsNav.select(tabMap[tab])
  }

  updateMessage(response: OrderReorderResponse): void {
    if (response.InvalidLi.length && !response.ValidLi.length) {
      this.message.string =
        'None of the line items on this order are available for reorder.'
      this.message.classType = 'danger'
      return
    }
    if (response.InvalidLi.length && response.ValidLi.length) {
      this.message.string =
        '<strong>Warning</strong> The following line items are not available for reorder, clicking add to cart will <strong>only</strong> add valid line items.'
      this.message.classType = 'warning'
      return
    }
    this.message.string = 'All line items are valid to reorder'
    this.message.classType = 'success'
  }

  async addToCart(): Promise<void> {
    await this.context.order.cart.AddValidLineItemsToCart(
      this.reorderResponse.ValidLi
    )
  }

  async moveOrderToCart(): Promise<void> {
    await this.context.order.cart.moveOrderToCart(this.order.ID)
  }

  onReturnCreated(): void {
    this.ngOnInit()
    this.showRequestReturn = false
    this.openNavItem('orderreturns')
  }

  protected createAndSavePDF(): void {
    this.context.pdfService.createAndSavePDF(this.order.ID)
  }
}
