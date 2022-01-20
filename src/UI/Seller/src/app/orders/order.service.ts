import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { Order, OcOrderService } from '@ordercloud/angular-sdk'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { Orders } from 'ordercloud-javascript-sdk'
import { OrderType } from '@app-seller/models/order.types'
import { HeadStartSDK, HSOrder } from '@ordercloud/headstart-sdk'
import { MiddlewareAPIService } from '@app-seller/shared/services/middleware-api/middleware-api.service'

@Injectable({
  providedIn: 'root',
})
export class OrderService extends ResourceCrudService<Order> {
  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    currentUserService: CurrentUserService,
    private middleware: MiddlewareAPIService
  ) {
    super(
      router,
      activatedRoute,
      Orders,
      currentUserService,
      '/orders',
      'orders'
    )
  }
  setOrderDirection(orderDirection: 'Incoming' | 'Outgoing') {
    this.patchFilterState({ OrderDirection: orderDirection })
  }
  isQuoteOrder(order: Order) {
    return order?.xp?.OrderType === OrderType.Quote
  }

  isSupplierOrder(orderID: string) {
    return orderID.split('-').length > 1
  }

  async list(args: any[]): Promise<any> {
    const filters = args.find((arg) => arg?.filters != null)
    if (this.router.url.includes('xp.OrderType=Quote')) {
      return await HeadStartSDK.Orders.ListQuoteOrders(
        filters?.filters['xp.QuoteStatus']
      )
    }
    return await super.list(args)
  }

  async getResourceById(resourceID: string): Promise<any> {
    try {
      const resource = await super.getResourceById(resourceID)
      return resource
    } catch (ex) {
      return await HeadStartSDK.Orders.GetQuoteOrder(resourceID)
    }
  }
}
