import { Component, OnInit, OnDestroy } from '@angular/core'
import { HSOrder, ListPage } from '@ordercloud/headstart-sdk'
import { ShopperContextService } from '../services/shopper-context/shopper-context.service'
import { OrderFilterService } from '../services/order-history/order-filter.service'
import { ActivatedRoute } from '@angular/router'
import { takeWhile } from 'rxjs/operators'

@Component({
  template: '<ocm-order-history [orders]="orders"></ocm-order-history>',
})
export class OrderHistoryWrapperComponent implements OnInit, OnDestroy {
  orders: ListPage<HSOrder>
  alive = true
  constructor(
    public context: ShopperContextService,
    private orderFilters: OrderFilterService,
    private router: ActivatedRoute
  ) {}

  ngOnInit(): void {
    this.orderFilters.activeFiltersSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe(this.setOrders)
  }

  setOrders = async (): Promise<void> => {
    this.orders = await this.orderFilters.listOrders()
  }

  ngOnDestroy(): void {
    this.alive = false
  }
}
