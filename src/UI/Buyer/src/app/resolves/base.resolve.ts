import { Injectable } from '@angular/core'
import { Resolve } from '@angular/router'
import { CurrentUserService } from '../services/current-user/current-user.service'
import { ExchangeRatesService } from '../services/exchange-rates/exchange-rates.service'
import { OrdersToApproveStateService } from '../services/order-history/order-to-approve-state.service'
import { CurrentOrderService } from '../services/order/order.service'
import { ProductCategoriesService } from '../services/product-categories/product-categories.service'
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
    private staticPageService: StaticPageService
  ) {}

  async resolve(): Promise<void> {
    await this.currentUser.reset()
    const order = this.currentOrder.reset()
    const ordersToApprove = this.ordersToApprove.reset()
    const categories = this.productCategories.setCategories()
    const exchangeRates = this.exchangeRates.reset()
    this.staticPageService.initialize()
    await Promise.all([order, ordersToApprove, categories, exchangeRates])
  }
}
