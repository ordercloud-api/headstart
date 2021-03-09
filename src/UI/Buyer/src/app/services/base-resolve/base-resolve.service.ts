import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { ToastrService } from "ngx-toastr";
import { OrderReorderResponse } from "src/app/models/order.types";
import { CurrentUserService } from "../current-user/current-user.service";
import { ExchangeRatesService } from "../exchange-rates/exchange-rates.service";
import { OrderHistoryService } from "../order-history/order-history.service";
import { OrdersToApproveStateService } from "../order-history/order-to-approve-state.service";
import { CartService } from "../order/cart.service";
import { OrderStateService } from "../order/order-state.service";
import { CurrentOrderService } from "../order/order.service";
import { ProductCategoriesService } from "../product-categories/product-categories.service";
import { ShopperContextService } from "../shopper-context/shopper-context.service";
import { StaticPageService } from "../static-page/static-page.service";


@Injectable()
export class BaseResolveService {
    constructor(
        private currentOrder: CurrentOrderService,
        private currentUser: CurrentUserService,
        private exchangeRates: ExchangeRatesService,
        private ordersToApprove: OrdersToApproveStateService,
        private productCategories: ProductCategoriesService,
        private staticPageService: StaticPageService,
        private orderService: OrderStateService,
        private toastrService: ToastrService,
        private orderHistory: OrderHistoryService,
        private cartService: CartService,
        private router: Router
    ){}

    async resolve(): Promise<void> {
        await this.currentUser.reset()
        var reorderResponse: OrderReorderResponse
        const anonLineItems = this.orderService.lineItems
        const anonOrder = this.orderService.order;
        const isAnon = this.currentUser.isAnonymous();
        if(anonOrder.ID && !isAnon) {
          reorderResponse = await this.orderHistory.validateReorder(
            anonOrder?.ID,
            anonLineItems?.Items
          )
        }
    
        
        const order = this.currentOrder.reset()
        
        const ordersToApprove = this.ordersToApprove.reset()
        const categories = this.productCategories.setCategories()
        const exchangeRates = this.exchangeRates.reset()
        this.staticPageService.initialize()
        await Promise.all([order, ordersToApprove, categories, exchangeRates])
        if(reorderResponse && reorderResponse.ValidLi.length) {
          const hasPreviousItems = this.orderService.lineItems && this.orderService.lineItems.Items?.length
          await this.cartService.AddValidLineItemsToCart(reorderResponse.ValidLi)
          if(hasPreviousItems) {
            this.toastrService.success('You had items in your cart from a previous visit. Please review your updated cart here.',
            'Cart Combined')
          }
        }
      }
}

