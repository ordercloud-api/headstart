import { Injectable } from "@angular/core";
import { Router } from "@angular/router";
import { HSLineItem } from "@ordercloud/headstart-sdk";
import { isEqual } from "lodash";
import { ToastrService } from "ngx-toastr";
import { ListPage } from "ordercloud-javascript-sdk";
import { OrderReorderResponse } from "src/app/models/order.types";
import { CurrentUserService } from "../current-user/current-user.service";
import { ExchangeRatesService } from "../exchange-rates/exchange-rates.service";
import { OrderHistoryService } from "../order-history/order-history.service";
import { OrdersToApproveStateService } from "../order-history/order-to-approve-state.service";
import { CartService } from "../order/cart.service";
import { OrderStateService } from "../order/order-state.service";
import { CurrentOrderService } from "../order/order.service";
import { ProductCategoriesService } from "../product-categories/product-categories.service";
import { SitecoreCDPTrackingService } from "../sitecore-cdp/sitecore-cdp-tracking.service";


@Injectable()
export class BaseResolveService {
  constructor(
    private currentOrder: CurrentOrderService,
    private currentUser: CurrentUserService,
    private exchangeRates: ExchangeRatesService,
    private ordersToApprove: OrdersToApproveStateService,
    private productCategories: ProductCategoriesService,
    private orderService: OrderStateService,
    private toastrService: ToastrService,
    private orderHistory: OrderHistoryService,
    private cartService: CartService,
    private router: Router,
    private cdp: SitecoreCDPTrackingService
  ) { }

  async resolve(): Promise<void> {
    await this.currentUser.reset()
    this.cdp.identify();
    var reorderResponse: OrderReorderResponse
    const anonLineItems = this.orderService.lineItems
    const anonOrder = this.orderService.order
    if (this.shouldTransferCart(anonLineItems)) {
      await this.refreshData();
      reorderResponse = await this.orderHistory.validateReorder(
        anonOrder?.ID,
        anonLineItems?.Items
      )
      const currentLineItems = this.orderService.lineItems;
      if (reorderResponse && reorderResponse.ValidLi.length) {
        const merged = await this.cartService.AddValidLineItemsToCart(reorderResponse.ValidLi)
        if (anonLineItems?.Items?.length > 0 &&
          currentLineItems?.Items?.length > 0 &&
          !this.lineItemsEqual(anonLineItems.Items, merged)) {
          this.toastrService.success('You had items in your cart from a previous visit. Please review your updated cart.')
          this.router.navigateByUrl('/cart')
        }
      } else if (reorderResponse && reorderResponse.InvalidLi) {
        this.toastrService.warning("Some items previously in your cart were not eligible for this user.")
        this.router.navigateByUrl('/cart')
      }
    } else {
      await this.refreshData()
    }
    this.currentUser.isAnonSubject.next(
      this.currentUser.isAnonymous()
    );
  }

  private shouldTransferCart(anonLineItems: ListPage<HSLineItem>): boolean {
    const isAnon = this.currentUser.isAnonymous() // user is not anonymous
    const wasAnon = this.currentUser.isAnonSubject.value;
    return !isAnon && wasAnon && anonLineItems?.Items?.length > 0;
  }

  private async refreshData(): Promise<void> {
    const order = this.currentOrder.reset()
    const ordersToApprove = this.ordersToApprove.reset()
    const categories = this.productCategories.setCategories()
    const exchangeRates = this.exchangeRates.reset()
    await Promise.all([order, ordersToApprove, categories, exchangeRates])
  }


  private lineItemsEqual(anonLis: HSLineItem[], mergedLis: HSLineItem[]): boolean {
    var equal = true;
    mergedLis.forEach(merged => {
      if (anonLis.some(anon => !isEqual(anon, merged))) {
        equal = false;
      }
    })
    return equal
  }
}

