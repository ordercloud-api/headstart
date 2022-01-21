import { Injectable } from '@angular/core'
import { BehaviorSubject } from 'rxjs'
import { Router, Params, ActivatedRoute } from '@angular/router'
import { CurrentUserService } from '../current-user/current-user.service'
import { Me, Sortable, Tokens } from 'ordercloud-javascript-sdk'
import { filter } from 'rxjs/operators'
import { HeadStartSDK, ListPage, HSOrder } from '@ordercloud/headstart-sdk'
import { ListArgs } from '@ordercloud/headstart-sdk'
import { HttpClient, HttpHeaders, HttpParams } from '@angular/common/http'
import { AppConfig } from 'src/app/models/environment.types'
import {
  OrderFilters,
  OrderViewContext,
  HeadstartOrderStatus,
} from 'src/app/models/order.types'

@Injectable({
  providedIn: 'root',
})
export class OrderFilterService {
  activeOrderID: string // TODO - make this read-only in components

  public activeFiltersSubject: BehaviorSubject<OrderFilters> = new BehaviorSubject<OrderFilters>(
    this.getDefaultParms()
  )

  constructor(
    private currentUser: CurrentUserService,
    private activatedRoute: ActivatedRoute,
    private router: Router,
    private http: HttpClient,
    private appConfig: AppConfig
  ) {
    this.activatedRoute.queryParams
      .pipe(filter(() => this.router.url.startsWith('/orders')))
      .subscribe(this.readFromUrlQueryParams)
  }

  buildHeaders(): HttpHeaders {
    return new HttpHeaders({
      Authorization: `Bearer ${Tokens.GetAccessToken()}`,
    })
  }

  createHttpParams(args: ListArgs): HttpParams {
    let params = new HttpParams()
    Object.entries(args).forEach(([key, value]) => {
      if (key !== 'filters' && value && value.toString() !== 'null') {
        params = params.append(key, value.toString())
      }
    })
    Object.entries(args.filters).forEach(([key, value]) => {
      if ((typeof value !== 'object' && value) || (value && value.length)) {
        params = params.append(key, value.toString())
      }
      if (key === 'xp' && value.SubmittedOrderStatus) {
        params = params.append(
          'xp.SubmittedOrderStatus',
          value.SubmittedOrderStatus.toString()
        )
      }
    })
    return params
  }

  toPage(pageNumber: number): void {
    this.patchFilterState({ page: pageNumber || undefined })
  }

  sortBy(field: Sortable<'Me.ListOrders'>): void {
    this.patchFilterState({ sortBy: field || undefined, page: undefined })
  }

  searchBy(searchTerm: string): void {
    this.patchFilterState({ search: searchTerm || undefined, page: undefined })
  }

  clearSearch(): void {
    this.patchFilterState({ search: undefined })
  }

  filterByFavorites(showOnlyFavorites: boolean): void {
    this.patchFilterState({ showOnlyFavorites, page: undefined })
  }

  filterByStatus(status: HeadstartOrderStatus): void {
    this.patchFilterState({ status: status || undefined, page: undefined })
  }

  filterByLocation(locationID: string): void {
    this.patchFilterState({
      location: locationID || undefined,
      page: undefined,
    })
  }

  filterByDateSubmitted(fromDate: string, toDate: string): void {
    this.patchFilterState({
      fromDate: fromDate || undefined,
      toDate: toDate || undefined,
    })
  }

  clearAllFilters(): void {
    this.patchFilterState(this.getDefaultParms())
  }

  // Used to update the URL
  mapToUrlQueryParams(model: OrderFilters): Params {
    const { page, sortBy, search, status, fromDate, toDate, location } = model
    const favorites = model.showOnlyFavorites ? 'true' : undefined
    return {
      page,
      sortBy,
      search,
      status,
      favorites,
      fromDate,
      toDate,
      location,
    }
  }

  // Used in requests to the OC API
  async listOrders(quote: boolean): Promise<ListPage<HSOrder>> {
    const viewContext = this.getOrderViewContext()
    switch (viewContext) {
      case OrderViewContext.MyOrders:
        return await Me.ListOrders(this.createListOptions(quote) as any)
      case OrderViewContext.Quote:
        return await Me.ListOrders(this.createListOptions(quote) as any)
      case OrderViewContext.Approve:
        return await Me.ListApprovableOrders(this.createListOptions() as any)
      case OrderViewContext.Location:
        // enforcing a location is selected before filtering
        if (this.activeFiltersSubject.value.location)
          return await this.ListLocationOrders()
    }
  }

  async ListLocationOrders(): Promise<ListPage<HSOrder>> {
    const locationID = this.activeFiltersSubject.value.location
    // Changed middleware route, awaiting next SDK bump (1.10.5?)
    // return await HeadStartSDK.Orders.ListLocationOrders(locationID, this.createListOptions() as any);
    const url = `${this.appConfig.middlewareUrl}/order/location/${locationID}`
    const args = this.createListOptions()
    const params = this.createHttpParams(args)
    return await this.http
      .get<ListPage<HSOrder>>(url, {
        headers: this.buildHeaders(),
        params,
      })
      .toPromise()
  }

  async listApprovableOrders(): Promise<ListPage<HSOrder>> {
    return await Me.ListApprovableOrders(this.createListOptions() as any)
  }

  private readFromUrlQueryParams = (params: Params): void => {
    const { page, sortBy, search, fromDate, toDate, location } = params
    const status = params.status
    const showOnlyFavorites = !!params.favorites
    this.activeFiltersSubject.next({
      page,
      sortBy,
      search,
      showOnlyFavorites,
      status,
      fromDate,
      toDate,
      location,
    })
  }

  getOrderViewContext(): string {
    const url = this.router.url
    if (url.includes('orders/approve')) {
      return OrderViewContext.Approve
    }
    if (url.includes('orders/location')) {
      return OrderViewContext.Location
    }
    if (url.includes('orders/quotes')) {
      return OrderViewContext.Quote
    }
    return OrderViewContext.MyOrders
  }

  private getDefaultParms(): OrderFilters {
    // default params are grabbed through a function that returns an anonymous object to avoid pass by reference bugs
    return {
      page: undefined,
      sortBy: undefined,
      search: undefined,
      status: undefined,
      showOnlyFavorites: false,
      fromDate: undefined,
      location: undefined,
      toDate: undefined,
    }
  }

  private createListOptions(quote: boolean = false): ListArgs<HSOrder> {
    const {
      page,
      sortBy,
      search,
      showOnlyFavorites,
      status,
      fromDate,
      toDate,
    } = this.activeFiltersSubject.value
    const from = fromDate ? `${fromDate}` : undefined
    const to = toDate ? `${toDate}` : undefined
    const favorites =
      this.currentUser.get().FavoriteOrderIDs.join('|') || undefined
    const quoteStatus = quote ? "NeedsSellerReview|NeedsBuyerReview" : undefined
    const listOptions = {
      page,
      search,
      sortBy,
      filters: {
        ID: showOnlyFavorites ? favorites : undefined,
        from,
        to,
        xp: {
          SubmittedOrderStatus: undefined,
          QuoteStatus: quoteStatus
        },
        Status: undefined,
      },
    }
    return this.addStatusFilters(status, listOptions as any)
  }

  private addStatusFilters(status: string, listOptions: ListArgs): ListArgs {
    if (status === HeadstartOrderStatus.ChangesRequested) {
      listOptions.filters.DateDeclined = '*'
    } else if (
      status === HeadstartOrderStatus.AwaitingApproval ||
      status === HeadstartOrderStatus.AllSubmitted
    ) {
      listOptions.filters.Status = status
    } else {
      listOptions.filters.xp.SubmittedOrderStatus = status
    }
    return listOptions
  }

  private patchFilterState(patch: OrderFilters): void {
    if (this.isNewDateFilter(patch, this.activeFiltersSubject.value))
      patch.page = undefined
    const activeFilters = { ...this.activeFiltersSubject.value, ...patch }
    const queryParams = this.mapToUrlQueryParams(activeFilters)
    this.router.navigate([], { queryParams }) // update url, which will call readFromUrlQueryParams()
  }

  private isNewDateFilter(
    patch: OrderFilters,
    activeFilters: OrderFilters
  ): boolean {
    let isNewDateFilter = false
    if (
      Object.keys(patch).includes('toDate') ||
      Object.keys(patch).includes('fromDate')
    ) {
      isNewDateFilter =
        patch.toDate !== activeFilters.toDate ||
        patch.fromDate !== activeFilters.fromDate
    }
    return isNewDateFilter
  }
}
