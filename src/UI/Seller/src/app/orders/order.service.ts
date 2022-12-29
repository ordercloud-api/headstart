import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { Orders, Order } from 'ordercloud-javascript-sdk'
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
  setOrderDirection(orderDirection: 'Incoming' | 'Outgoing'): void {
    this.patchFilterState({ OrderDirection: orderDirection })
  }
  isQuoteOrder(order: HSOrder): boolean {
    return order?.xp?.OrderType === OrderType.Quote
  }

  isSupplierOrder(orderID: string): boolean {
    return orderID.split('-').length > 1
  }

  async list(args: any[]): Promise<any> {
    if (this.router.url.includes('xp.OrderType=Quote')) {
      return await HeadStartSDK.Orders.ListQuoteOrders(args[1])
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
