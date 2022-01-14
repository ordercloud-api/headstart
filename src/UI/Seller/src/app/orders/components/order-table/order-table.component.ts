import { Component, ChangeDetectorRef, NgZone } from '@angular/core'
import { ResourceCrudComponent } from '@app-seller/shared/components/resource-crud/resource-crud.component'
import { Order } from '@ordercloud/angular-sdk'
import { Router, ActivatedRoute, Params } from '@angular/router'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { OrderService } from '@app-seller/orders/order.service'
import { SELLER } from '@app-seller/models/user.types'

@Component({
  selector: 'app-order-table',
  templateUrl: './order-table.component.html',
  styleUrls: ['./order-table.component.scss'],
})
export class OrderTableComponent extends ResourceCrudComponent<Order> {
  isListPage: boolean
  shouldShowOrderToggle = false
  activeOrderDirectionButton: string
  isQuoteOrderList = false
  constructor(
    private orderService: OrderService,
    changeDetectorRef: ChangeDetectorRef,
    router: Router,
    activatedroute: ActivatedRoute,
    ngZone: NgZone,
    private appAuthService: AppAuthService
  ) {
    super(changeDetectorRef, orderService, router, activatedroute, ngZone)
    this.shouldShowOrderToggle =
      this.appAuthService.getOrdercloudUserType() === SELLER
    activatedroute.queryParams.subscribe((params) => {
      if (this.router.url.startsWith('/orders')) {
        this.readFromUrlQueryParams(params)
      }
    })
    activatedroute.params.subscribe((params) => {
      this.isListPage = !params.orderID
    })
  }
  setOrderDirection(direction: 'Incoming' | 'Outgoing') {
    if (this.isListPage) {
      this.orderService.setOrderDirection(direction)
    } else {
      this.router.navigate(['/orders'], {
        queryParams: { OrderDirection: direction },
      })
    }
  }

  private readFromUrlQueryParams(params: Params): void {
    const { OrderDirection } = params
    this.isQuoteOrderList = params['xp.OrderType'] === 'Quote'
    this.activeOrderDirectionButton = OrderDirection
    if (this.isQuoteOrderList) {
      this.filterConfig = {
        Filters: [
          {
            Display: 'Quote Status',
            Path: 'xp.QuoteStatus',
            Items: [
              { Text: 'Needs Your Review', Value: 'NeedsSellerReview' },
              { Text: 'Needs Buyer Review', Value: 'NeedsBuyerReview' },
            ],
            Type: 'Dropdown',
          },
        ],
      }
    }
  }
  filterConfig = {
    Filters: [
      {
        Display: 'ADMIN.FILTERS.STATUS',
        Path: 'xp.SubmittedOrderStatus',
        Items: [
          { Text: 'ADMIN.FILTER_OPTIONS.OPEN', Value: 'Open' },
          { Text: 'ADMIN.FILTER_OPTIONS.COMPLETED', Value: 'Completed' },
          { Text: 'ADMIN.FILTER_OPTIONS.CANCELED', Value: 'Canceled' },
        ],
        Type: 'Dropdown',
      },
      {
        Display: 'ADMIN.FILTERS.FROM_DATE',
        Path: 'from',
        Type: 'DateFilter',
      },
      {
        Display: 'ADMIN.FILTERS.TO_DATE',
        Path: 'to',
        Type: 'DateFilter',
      },
      {
        Display: 'ADMIN.FILTERS.SELLER_OWNED_PRODUCTS',
        Path: 'xp.HasSellerProducts',
        Items: [
          { Text: 'ADMIN.FILTER_OPTIONS.YES', Value: 'true' },
          { Text: 'ADMIN.FILTER_OPTIONS.NO', Value: 'false|!*' },
        ],
        Type: 'Dropdown',
        QueryRestriction: 'OrderDirection=Incoming',
      },
      {
        Display: 'ADMIN.FILTERS.HAS_CLAIMS',
        Path: 'xp.OrderReturnInfo.HasReturn',
        Items: [{ Value: true }, { Value: false }],
        Type: 'Dropdown',
        QueryRestriction: 'OrderDirection=Incoming',
      },
    ],
  }
}
