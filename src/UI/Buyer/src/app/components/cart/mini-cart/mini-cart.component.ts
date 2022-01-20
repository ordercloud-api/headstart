import { Component, Output, EventEmitter, OnInit } from '@angular/core'
import { faEllipsisH } from '@fortawesome/free-solid-svg-icons'
import { HSOrder, HSLineItem } from '@ordercloud/headstart-sdk'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './mini-cart.component.html',
})
export class OCMMiniCart implements OnInit {
  @Output() navigate = new EventEmitter() // to do, use context on pathChange instead?
  lineItems: HSLineItem[] = []
  order: HSOrder = {}
  maxLines = 5 // Limit the height for UI purposes
  faEllipsisH = faEllipsisH
  _orderCurrency: string

  constructor(private context: ShopperContextService) {}

  ngOnInit(): void {
    this.order = this.context.order.get()
    this.lineItems = this.context.order.cart.get().Items
    this._orderCurrency = this.context.currentUser.get().Currency
  }

  toFullCart(): void {
    this.context.router.toCart()
    this.navigate.emit()
  }

  toProductDetails(
    productID: string,
    configurationID: string,
    documentID: string
  ): void {
    this.context.router.toProductDetails(productID)
    this.navigate.emit()
  }

  toCheckout(): void {
    this.context.router.toCheckout()
    this.navigate.emit()
  }

  isQuoteOrder(): boolean {
    return this.order?.xp?.OrderType === 'Quote'
  }
}
