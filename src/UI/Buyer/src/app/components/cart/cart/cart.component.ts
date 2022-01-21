import { Component, Input, OnDestroy, OnInit } from '@angular/core'
import { ListPage, OrderPromotion } from 'ordercloud-javascript-sdk'
import { HSLineItem, HSOrder } from '@ordercloud/headstart-sdk'
import { getOrderSummaryMeta } from 'src/app/services/purchase-order.helper'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { OrderSummaryMeta } from 'src/app/models/order.types'
import { LineItemWithProduct } from 'src/app/models/line-item.types'
import { ModalState } from 'src/app/models/shared.types'
import { faShoppingCart } from '@fortawesome/free-solid-svg-icons'
import { getPrimaryLineItemImage } from 'src/app/services/images.helpers'
import { CurrentOrderService } from 'src/app/services/order/order.service'

@Component({
  templateUrl: './cart.component.html',
  styleUrls: ['./cart.component.scss'],
})
export class OCMCart implements OnInit, OnDestroy {
  _order: HSOrder
  _orderPromos: ListPage<OrderPromotion>
  _lineItems: ListPage<LineItemWithProduct>
  _invalidLineItems: HSLineItem[] = []
  invalidLineItemCount: number
  invalidLineItemsAreBeingRemoved: boolean
  orderSummaryMeta: OrderSummaryMeta
  orderErrorModal = ModalState.Closed
  orderError: string
  faShoppingCart = faShoppingCart
  _isCartValid = true
  isEmptyingCart = false
  @Input() set invalidLineItems(value: HSLineItem[]) {
    this._invalidLineItems = value
    if (
      this._invalidLineItems.length &&
      !this.invalidLineItemsAreBeingRemoved
    ) {
      this.orderError =
        "We're sorry, but some items in your cart are not available."
      this.orderErrorModal = ModalState.Open
    }
  }
  @Input() set order(value: HSOrder) {
    this._order = value
    if (this._order) {
      this.setOrderSummaryMeta()
    }
  }
  @Input() set lineItems(value: ListPage<LineItemWithProduct>) {
    this._lineItems = value
    if (this._lineItems) {
      this.setOrderSummaryMeta()
    }
  }

  @Input() set orderPromos(value: ListPage<OrderPromotion>) {
    this._orderPromos = value
    if (this._orderPromos) {
      this.setOrderSummaryMeta()
    }
  }

  constructor(
    private context: ShopperContextService,
    private currentOrder: CurrentOrderService
  ) {}

  ngOnInit(): void {
    this.currentOrder.cart.isCartValidSubject.subscribe((valid) => {
      this._isCartValid = valid
    })
  }

  setOrderSummaryMeta(): void {
    if (this._order && this._lineItems && this._orderPromos) {
      this.orderSummaryMeta = getOrderSummaryMeta(
        this._order,
        this._orderPromos?.Items,
        this._lineItems.Items,
        'cart'
      )
    }
  }
  toProductList(): void {
    this.context.router.toProductList()
  }

  toCheckout(): void {
    this.context.router.toCheckout()
  }

  async submitQuote(): Promise<void> {
    var currentOrder = this.context.order.get()
    currentOrder.xp.QuoteStatus = 'NeedsSellerReview'
    currentOrder.xp.QuoteSubmittedDate = new Date().toISOString()
    await this.context.order.patch(currentOrder)
    await this.context.order.sendQuoteNotification(currentOrder.ID, this._lineItems.Items[0].ID)
    
    // The reset function does a search on xp.QuoteStatus.
    // The indexing appears to be happening in the background.
    // calling the function too soon results in the order remaining in the cart.
    setTimeout(async () => {
      await this.context.order.cart.reset()
      this.context.router.toMyQuotes()
    }, 1000)
  }

  emptyCart(): void {
    this.isEmptyingCart = true
    Promise.all([
      this.context.order.cart.empty(),
      this.context.order.promos.removeAllPromos(),
    ]).finally(() => (this.isEmptyingCart = false))
  }

  async removeInvalidLineItems(): Promise<void> {
    this.invalidLineItemsAreBeingRemoved = true
    this.orderErrorModal = ModalState.Closed
    for (const lineItem of this._invalidLineItems) {
      await this.context.order.cart.remove(lineItem.ID)
    }
    this.invalidLineItemsAreBeingRemoved = false
  }

  updateOrderMeta(): void {
    this.orderSummaryMeta = getOrderSummaryMeta(
      this._order,
      this._orderPromos?.Items,
      this._lineItems.Items,
      'cart'
    )
  }

  getImageUrl(lineItemID: string): string {
    return getPrimaryLineItemImage(
      lineItemID,
      this._invalidLineItems,
      this.context.currentUser.get()
    )
  }

  ngOnDestroy(): void {
    this.currentOrder.cart.isCartValidSubject.unsubscribe()
  }
}
