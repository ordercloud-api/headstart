import { Injectable } from '@angular/core'
import { Resolve, Router } from '@angular/router'
import { ToastrService } from 'ngx-toastr'
import { OrderReorderResponse } from '../models/order.types'
import { CurrentUserService } from '../services/current-user/current-user.service'
import { ExchangeRatesService } from '../services/exchange-rates/exchange-rates.service'
import { OrdersToApproveStateService } from '../services/order-history/order-to-approve-state.service'
import { OrderStateService } from '../services/order/order-state.service'
import { CurrentOrderService } from '../services/order/order.service'
import { ProductCategoriesService } from '../services/product-categories/product-categories.service'
import { ShopperContextService } from '../services/shopper-context/shopper-context.service'
import { StaticPageService } from '../services/static-page/static-page.service'

@Injectable({
  providedIn: 'root',
})
export class BaseResolve implements Resolve<any> {
  constructor(
    private currentOrder: CurrentOrderService,
    private currentUser: CurrentUserService,
    private exchangeRates: ExchangeRatesService,
    private ordersToApprove: OrdersToApproveStateService,
    private productCategories: ProductCategoriesService,
    private staticPageService: StaticPageService,
    private orderService: OrderStateService,
    private context: ShopperContextService,
    private toastrService: ToastrService,
    private router: Router
  ) {}

  async resolve(): Promise<void> {
    var reorderResponse: OrderReorderResponse
    const anonLineItems = this.orderService.lineItems
    const anonOrder = this.orderService.order;
    if(anonOrder.ID) {
      reorderResponse = await this.context.orderHistory.validateReorder(
        anonOrder?.ID,
        anonLineItems?.Items
      )
    }

    await this.currentUser.reset()
    const order = this.currentOrder.reset()
    
    const ordersToApprove = this.ordersToApprove.reset()
    const categories = this.productCategories.setCategories()
    const exchangeRates = this.exchangeRates.reset()
    this.staticPageService.initialize()
    await Promise.all([order, ordersToApprove, categories, exchangeRates])
    if(reorderResponse && reorderResponse.ValidLi.length) {
      const hasPreviousItems = this.orderService.lineItems && this.orderService.lineItems.Items?.length
      await this.context.order.cart.AddValidLineItemsToCart(reorderResponse.ValidLi)
      if(hasPreviousItems) {
        this.toastrService.success('You had items in your cart from a previous visit. Please review your updated cart here.',
        'Cart Combined')
      }
    }
  }
}
