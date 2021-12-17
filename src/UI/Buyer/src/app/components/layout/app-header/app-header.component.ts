import { Component, ViewChild, ElementRef, OnInit } from '@angular/core'
import { animate, style, transition, trigger } from '@angular/animations'
import {
  faSearch,
  faShoppingCart,
  faPhone,
  faQuestionCircle,
  faUserCircle,
  faSignOutAlt,
  faBoxOpen,
  faHome,
  faBars,
  faTimes,
} from '@fortawesome/free-solid-svg-icons'
import { NgbPopover } from '@ng-bootstrap/ng-bootstrap'
import { Category, ListPage, Supplier } from 'ordercloud-javascript-sdk'
import { bufferTime, filter, takeWhile } from 'rxjs/operators'
import { HSOrder, HSLineItem } from '@ordercloud/headstart-sdk'
import { getScreenSizeBreakPoint } from 'src/app/services/breakpoint.helper'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { CurrentUser } from 'src/app/models/profile.types'
import { AppConfig } from 'src/app/models/environment.types'
import { ProductFilters } from 'src/app/models/filter-config.types'
import { RouteConfig } from 'src/app/models/shared.types'
import { SitecoreCDPTrackingService } from 'src/app/services/sitecore-cdp/sitecore-cdp-tracking.service'

@Component({
  templateUrl: './app-header.component.html',
  styleUrls: ['./app-header.component.scss'],
  animations: [
    trigger('inOutAnimation', [
      transition(':enter', [
        style({ transform: 'translateX(-100%)', opacity: 0 }),
        animate(
          '.4s cubic-bezier(0.7, 0, 0.3, 1)',
          style({ transform: 'translateX(0)', opacity: 1 })
        ),
      ]),
      transition(':leave', [
        style({ transform: 'translateX(0)', opacity: 1 }),
        animate(
          '.2s cubic-bezier(0.7, 0, 0.3, 1)',
          style({ transform: 'translateX(-100%)', opacity: 0 })
        ),
      ]),
    ]),
  ],
})
export class OCMAppHeader implements OnInit {
  isCollapsed = true
  isAnonymous: boolean
  isSSO: boolean
  user: CurrentUser
  order: HSOrder
  alive = true
  addToCartQuantity: number
  searchTermForProducts: string = null
  activePath: string
  appName: string
  activeCategoryID: string = undefined
  categories: Category[] = []
  screenSize = getScreenSizeBreakPoint()
  showCategoryDropdown = false
  profileRoutes: RouteConfig[] = []
  orderRoutes: RouteConfig[] = []
  numberOfOrdersToApprove = 0

  @ViewChild('addToCartPopover', { static: false })
  public addToCartPopover: NgbPopover
  @ViewChild('ordersToApprovePopover', { static: false })
  public ordersToApprovePopover: NgbPopover
  @ViewChild('cartIcon', { static: false }) cartIcon: ElementRef

  faSearch = faSearch
  faShoppingCart = faShoppingCart
  faPhone = faPhone
  faQuestionCircle = faQuestionCircle
  faSignOutAlt = faSignOutAlt
  faUserCircle = faUserCircle
  faHome = faHome
  faBars = faBars
  faTimes = faTimes
  faBoxOpen = faBoxOpen
  flagIcon: string
  hasSuppliers = false;
  currentSupplierList: ListPage<Supplier>

  constructor(
    public context: ShopperContextService,
    public appConfig: AppConfig,
    private cdp: SitecoreCDPTrackingService
  ) {
    this.profileRoutes = context.router.getProfileRoutes()
    this.orderRoutes = context.router.getOrderRoutes()
  }

  async ngOnInit(): Promise<void> {
    this.buildShowOrdersNeedingApprovalAlertListener()
    this.screenSize = getScreenSizeBreakPoint()
    this.categories = this.context.categories.all
    this.appName = this.context.appSettings.appname
    this.activePath = this.context.router.getActiveUrl()
    this.isSSO = this.context.currentUser.isSSO()
    this.context.order.onChange((order) => (this.order = order))
    this.context.currentUser.onChange((user) => {
      this.user = user
      this.isAnonymous = this.context.currentUser.isAnonymous()
    })
    this.context.productFilters.activeFiltersSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe(this.handleFiltersChange)
    this.context.router.onUrlChange((path) => (this.activePath = path))
    this.buildAddToCartListener()
    this.flagIcon = this.getCurrencyFlag()
    this.currentSupplierList = await this.getCurrentSupplierList()
    this.hasSuppliers = this.currentSupplierList.Meta.TotalCount > 0
  }

  getCurrencyFlag(): string {
    const rates = this.context.exchangeRates.Get()
    const currentUser = this.context.currentUser.get()
    const myRate = rates?.Items.find((r) => r.Currency === currentUser.Currency)
    return myRate?.Icon
  }

  async getCurrentSupplierList() {
    this.setBuyerFilterIfNeeded();
    const supplierList: ListPage<Supplier> = await this.context.supplierFilters.listSuppliers()

    return supplierList;
  }

  private setBuyerFilterIfNeeded(): void {
    this.context.supplierFilters.setNonURLFilter(
      'xp.BuyersServicing',
      this.context.currentUser.get()?.Buyer?.ID
    )
  }

  toggleCategoryDropdown(bool: boolean): void {
    this.showCategoryDropdown = bool
  }

  clickOutsideCategoryDropdown(event: any): void {
    const clickIsOutside = !event.target.closest('.category-nav__menu')
    if (clickIsOutside) {
      this.showCategoryDropdown = false
    }
  }

  handleFiltersChange = (filters: ProductFilters): void => {
    this.searchTermForProducts = filters.search || ''
    this.activeCategoryID = this.context.categories.activeID
  }

  buildAddToCartListener(): void {
    let closePopoverTimeout
    this.context.order.cart.onAdd
      .pipe(
        bufferTime(500),
        filter((lines) => Boolean(lines.length))
      )
      .subscribe((lis: HSLineItem[]) => {
        clearTimeout(closePopoverTimeout)
        if (lis.length) {
          let totalQuantity = lis
            .map((li) => li.Quantity)
            .reduce((accum, currentVal) => accum + currentVal, 0)
          this.addToCartPopover.ngbPopover = `Added ${totalQuantity} items to Cart`
          setTimeout(() => {
            if (!this.addToCartPopover.isOpen()) {
              this.addToCartPopover.open()
            }
            closePopoverTimeout = setTimeout(() => {
              this.addToCartPopover.close()
              totalQuantity = 0
            }, 3000)
          }, 300)
        }
      })
  }

  routeToOrdersToApprove(event: any): void {
    this.context.router.toOrdersToApprove()
    event.stopPropagation()
  }

  buildShowOrdersNeedingApprovalAlertListener(): void {
    this.buildShowOrdersNeedingApprovalListenerForAlert()
    this.buildShowOrdersNeedingApprovalListenerForIndicator()
  }

  buildShowOrdersNeedingApprovalListenerForIndicator(): void {
    this.context.ordersToApprove.numberOfOrdersToApprove.subscribe((num) => {
      this.numberOfOrdersToApprove = num
    })
  }

  buildShowOrdersNeedingApprovalListenerForAlert(): void {
    /* the orders to approve behaviour subject will only receive a truthy
     * value when OrdersToApproveStateService.alertIfOrdersToApprove is called.
     * This currently only happens on login. In order to prevent the alert from
     * showing again we set the value back to zero when this is called
     */
    this.context.ordersToApprove.showAlert.subscribe((num: number) => {
      // brief timeout to allow the popover to be defined in the template
      // additionally i think this slight timeout is a better ux
      setTimeout(() => {
        if (num) {
          this.numberOfOrdersToApprove = num
          this.context.ordersToApprove.showAlert.next(0)
          this.ordersToApprovePopover.open()
          setTimeout(() => {
            this.ordersToApprovePopover.close()
          }, 10000)
        }
      }, 1500)
    })
  }
  searchProducts(searchStr: string): void {
    this.searchTermForProducts = searchStr
    this.cdp.productSearched(searchStr); 
    this.context.router.toProductList({ search: searchStr })
  }

  isRouteActive(url: string): boolean {
    return (
      this.activePath === url ||
      (this.activePath === '/profile' && url === '/profile/details')
    )
  }

  logout(): void {
    this.context.authentication.logout()
  }

  closeMiniCart(event: MouseEvent, popover: NgbPopover): void {
    const rect = this.cartIcon.nativeElement.getBoundingClientRect()
    // do not close if leaving through the bottom. That is handled by minicart itself
    if (event.y < rect.top + rect.height) {
      popover.close()
    }
  }

  setActiveCategory(categoryID: string): void {
    this.context.productFilters.filterByCategory(categoryID)
  }

  toggleCollapsed(): void {
    this.isCollapsed = !this.isCollapsed
  }
}
