// angular
import { Injectable } from '@angular/core'
import { OrderStateService } from './order-state.service'
import { CartService } from './cart.service'
import { CheckoutService } from './checkout.service'
import {
  LineItems,
  Orders,
  Order,
  LineItem,
  IntegrationEvents,
  ListPage,
} from 'ordercloud-javascript-sdk'
import {
  HSOrder,
  HSLineItem,
  QuoteOrderInfo,
  HeadStartSDK,
} from '@ordercloud/headstart-sdk'
import { PromoService } from './promo.service'
import { AppConfig } from 'src/app/models/environment.types'
import { OrderType } from 'src/app/models/order.types'

@Injectable({
  providedIn: 'root',
})
export class CurrentOrderService {
  onChange: (callback: (order: HSOrder) => void) => void
  reset: () => Promise<void>
  constructor(
    private cartService: CartService,
    private promoService: PromoService,
    private checkoutService: CheckoutService,
    private state: OrderStateService,
    private appConfig: AppConfig
  ) {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
    this.onChange = this.state.onOrderChange.bind(this.state)
    // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
    this.reset = this.state.reset.bind(this.state)
  }

  get(): HSOrder {
    return this.state.order
  }

  getLineItems(): ListPage<HSLineItem> {
    return this.state.lineItems
  }

  public async patch(order: HSOrder, orderID?: string): Promise<void> {
    await Orders.Patch(
      'Outgoing',
      orderID || order.ID,
      order
    )
  }

  public async delete(orderID: string): Promise<void> {
    await Orders.Delete('All', orderID)
  }

  public async sendQuoteNotification(orderID: string, lineItemID: string): Promise<void> {
    await HeadStartSDK.Orders.SendQuoteRequestToSupplier(orderID, lineItemID)
  }

  get cart(): CartService {
    return this.cartService
  }

  get promos(): PromoService {
    return this.promoService
  }

  get checkout(): CheckoutService {
    return this.checkoutService
  }
  
}
