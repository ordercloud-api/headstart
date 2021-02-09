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
} from 'ordercloud-javascript-sdk'
import {
  HSOrder,
  HSLineItem,
  QuoteOrderInfo,
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

  get cart(): CartService {
    return this.cartService
  }

  get promos(): PromoService {
    return this.promoService
  }

  get checkout(): CheckoutService {
    return this.checkoutService
  }

  async submitQuoteOrder(
    info: QuoteOrderInfo,
    lineItem: HSLineItem
  ): Promise<Order> {
    const order = this.buildQuoteOrder(info)
    lineItem.xp.StatusByQuantity = {
      Submitted: 0,
      Open: 1,
      Backordered: 0,
      Canceled: 0,
      CancelRequested: 0,
      Returned: 0,
      ReturnRequested: 0,
      Complete: 0,
    } as any
    const quoteOrder = await Orders.Create('Outgoing', order)
    await LineItems.Create('Outgoing', quoteOrder.ID, lineItem as LineItem)
    await IntegrationEvents.Calculate('Outgoing', quoteOrder.ID)
    const submittedQuoteOrder = await Orders.Submit('Outgoing', quoteOrder.ID)
    return submittedQuoteOrder
  }

  //todo revert type to QuoteOrderInfo
  buildQuoteOrder(info: any): Order {
    return {
      ID: `${this.appConfig.incrementorPrefix}{orderIncrementor}`,
      ShippingAddressID: info.ShippingAddressId,
      xp: {
        AvalaraTaxTransactionCode: '',
        OrderType: OrderType.Quote,
        QuoteOrderInfo: {
          FirstName: info.FirstName,
          LastName: info.LastName,
          BuyerLocation: (info as any).BuyerLocation,
          Phone: info.Phone,
          Email: info.Email,
          Comments: info.Comments,
        },
      },
    }
  }
}
