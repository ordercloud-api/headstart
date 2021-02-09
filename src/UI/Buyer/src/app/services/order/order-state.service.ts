import { Injectable } from '@angular/core'
import {
  ListPage,
  HSLineItem,
  HSOrder,
} from '@ordercloud/headstart-sdk'
import {
  LineItems,
  Me,
  Order,
  Orders,
  OrderPromotion,
  ShipEstimate,
  IntegrationEvents,
} from 'ordercloud-javascript-sdk'
import { BehaviorSubject } from 'rxjs'
import { AppConfig } from 'src/app/models/environment.types'
import { ClaimStatus } from 'src/app/models/order.types'
import { ShippingStatus } from 'src/app/models/shipping.types'
import { CurrentUserService } from '../current-user/current-user.service'
import { listAll } from '../listAll'
import { TokenHelperService } from '../token-helper/token-helper.service'

@Injectable({
  providedIn: 'root',
})
export class OrderStateService {
  public readonly DefaultLineItems: ListPage<HSLineItem> = {
    Meta: { Page: 1, PageSize: 25, TotalCount: 0, TotalPages: 1 },
    Items: [],
  }
  public readonly DefaultOrderPromos: ListPage<OrderPromotion> = {
    Meta: { Page: 1, PageSize: 25, TotalCount: 0, TotalPages: 1 },
    Items: [],
  }
  private readonly DefaultOrder: HSOrder = {
    xp: {
      AvalaraTaxTransactionCode: '',
      OrderType: 'Standard',
      QuoteOrderInfo: null,
      Currency: 'USD', // Default value, overriden in reset() when app loads
      Returns: {
        HasClaims: false,
        HasUnresolvedClaims: false,
        Resolutions: [],
      },
      ClaimStatus: ClaimStatus.NoClaim,
      ShippingStatus: ShippingStatus.Processing,
    },
  }
  private orderSubject = new BehaviorSubject<HSOrder>(
    this.DefaultOrder
  )
  private shipEstimatesSubject = new BehaviorSubject<ShipEstimate[]>([])
  private orderPromosSubject = new BehaviorSubject<ListPage<OrderPromotion>>(
    this.DefaultOrderPromos
  )
  private lineItemSubject = new BehaviorSubject<ListPage<HSLineItem>>(
    this.DefaultLineItems
  )

  constructor(
    private tokenHelper: TokenHelperService,
    private currentUserService: CurrentUserService,
    private appConfig: AppConfig
  ) {}

  get order(): HSOrder {
    return this.orderSubject.value
  }

  set order(value: HSOrder) {
    this.orderSubject.next(value)
  }

  get shipEstimates(): ShipEstimate[] {
    return this.shipEstimatesSubject.value
  }

  set shipEstimates(value: ShipEstimate[]) {
    this.shipEstimatesSubject.next(value)
  }

  get lineItems(): ListPage<HSLineItem> {
    return this.lineItemSubject.value
  }

  set lineItems(value: ListPage<HSLineItem>) {
    this.lineItemSubject.next(value)
  }

  get orderPromos(): ListPage<OrderPromotion> {
    return this.orderPromosSubject.value
  }

  set orderPromos(value: ListPage<OrderPromotion>) {
    this.orderPromosSubject.next(value)
  }

  onOrderChange(callback: (order: HSOrder) => void): void {
    this.orderSubject.subscribe(callback)
  }

  onLineItemsChange(
    callback: (lineItems: ListPage<HSLineItem>) => void
  ): void {
    this.lineItemSubject.subscribe(callback)
  }

  onPromosChange(callback: (promos: ListPage<OrderPromotion>) => void): void {
    this.orderPromosSubject.subscribe(callback)
  }

  async reset(): Promise<void> {
    /* when an order is declined it will appear as an unsubmitted order with
     * a date declined, we need to remove these from the normal order
     * query so that it does not immediately affect a users cart by getting
     * mixed into the normal unsubmitted orders
     * however we also need to know when a user marks an order for
     * resbumit which we are designating with xp.IsResubmitting
     */
    const [ordersForResubmit, ordersNeverSubmitted] = await Promise.all([
      this.getOrdersForResubmit(),
      this.getOrdersNeverSubmitted(),
    ])
    if (ordersForResubmit.Items.length) {
      this.order = ordersForResubmit.Items[0]
    } else if (ordersNeverSubmitted.Items.length) {
      this.order = ordersNeverSubmitted.Items[0]
    } else if (this.appConfig.anonymousShoppingEnabled) {
      this.order = { ID: this.tokenHelper.getAnonymousOrderID() }
    } else {
      this.DefaultOrder.xp.Currency = this.currentUserService.get().Currency
      this.order = (await Orders.Create(
        'Outgoing',
        this.DefaultOrder as Order
      )) as HSOrder
    }
    if (this.order.DateCreated) {
      await this.resetLineItems()
    }
    this.orderPromos = await Orders.ListPromotions('Outgoing', this.order.ID)

    await this.getShipEstimates()
  }

  async getShipEstimates(): Promise<void> {
    const orderWorksheet = await IntegrationEvents.GetWorksheet(
      'Outgoing',
      this.order.ID
    )

    if (orderWorksheet?.ShipEstimateResponse?.ShipEstimates) {
      this.shipEstimates = orderWorksheet.ShipEstimateResponse.ShipEstimates
    }
  }

  async resetLineItems(): Promise<void> {
    this.lineItems = await listAll(
      LineItems,
      LineItems.List,
      'outgoing',
      this.order.ID
    )
  }

  private async getOrdersForResubmit(): Promise<ListPage<HSOrder>> {
    const orders = await Me.ListOrders({
      sortBy: '!DateCreated',
      filters: {
        DateDeclined: '*',
        status: 'Unsubmitted',
        'xp.IsResubmitting': 'True',
      },
    })
    return orders
  }

  private async getOrdersNeverSubmitted(): Promise<ListPage<HSOrder>> {
    const orders = await Me.ListOrders({
      sortBy: '!DateCreated',
      filters: {
        DateDeclined: '!*',
        status: 'Unsubmitted',
        'xp.OrderType': 'Standard',
      },
    })
    return orders
  }
}
