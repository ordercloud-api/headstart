import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { Order, OcOrderService } from '@ordercloud/angular-sdk'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { Orders } from 'ordercloud-javascript-sdk'
import { OrderType } from '@app-seller/models/order.types'

@Injectable({
  providedIn: 'root',
})
export class OrderService extends ResourceCrudService<Order> {
  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    currentUserService: CurrentUserService
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
    return orderID.split("-").length > 1; 
  }
}
