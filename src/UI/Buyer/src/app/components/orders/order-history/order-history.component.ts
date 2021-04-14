import {
  Component,
  AfterViewInit,
  Input,
  OnDestroy,
  OnInit,
} from '@angular/core'
import { ListPage } from 'ordercloud-javascript-sdk'
import { takeWhile } from 'rxjs/operators'
import { HSOrder } from '@ordercloud/headstart-sdk'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import {
  OrderFilters,
  HeadstartOrderStatus,
  OrderViewContext,
} from 'src/app/models/order.types'
import { RouteConfig } from 'src/app/models/shared.types'

@Component({
  templateUrl: './order-history.component.html',
  styleUrls: ['./order-history.component.scss'],
})
export class OCMOrderHistory implements OnInit, AfterViewInit, OnDestroy {
  alive = true
  columns: string[] = ['ID', 'Status', 'DateSubmitted', 'Total']
  @Input() orders: ListPage<HSOrder>
  viewContext: string
  showOnlyFavorites = false
  sortBy: string
  searchTerm: string
  orderRoutes: RouteConfig[]

  constructor(private context: ShopperContextService) {}

  ngOnInit(): void {
    this.orderRoutes = this.context.router.getOrderRoutes()
    this.viewContext = this.context.orderHistory.filters.getOrderViewContext()
    this.context.orderHistory.filters.activeFiltersSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe(this.handleFiltersChange)
  }

  handleFiltersChange = (filters: OrderFilters): void => {
    this.sortBy = String(filters.sortBy)
    this.showOnlyFavorites = filters.showOnlyFavorites
    this.searchTerm = filters.search
  }

  ngAfterViewInit(): void {
    // cannot filter on favorite orders for orders to approve
    if (this.viewContext === OrderViewContext.MyOrders) {
      this.columns.push('Favorite')
    }
    if (this.viewContext === OrderViewContext.Location) {
      this.columns.push('Location')
    }
  }

  sortOrders(sortBy: string): void {
    this.context.orderHistory.filters.sortBy([sortBy as any])
  }

  changePage(page: number): void {
    this.context.orderHistory.filters.toPage(page)
  }

  filterBySearch(search: string): void {
    this.context.orderHistory.filters.searchBy(search)
  }

  filterByStatus(status: HeadstartOrderStatus): void {
    this.context.orderHistory.filters.filterByStatus(status)
  }

  filterByFavorite(showOnlyFavorites: boolean): void {
    this.context.orderHistory.filters.filterByFavorites(showOnlyFavorites)
  }

  ngOnDestroy(): void {
    this.alive = false
  }
}
