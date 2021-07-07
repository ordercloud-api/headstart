import { Component, Input, OnInit, OnDestroy } from '@angular/core'
import { ListFacet, Category, ListPage } from 'ordercloud-javascript-sdk'
import { faTimes } from '@fortawesome/free-solid-svg-icons'
import { getScreenSizeBreakPoint } from 'src/app/services/breakpoint.helper'
import { takeWhile } from 'rxjs/operators'
import { HSMeProduct } from '@ordercloud/headstart-sdk'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { ShipFromSourcesDic } from 'src/app/models/shipping.types'
import { ProductFilters } from 'src/app/models/filter-config.types'

@Component({
  templateUrl: './product-list.component.html',
  styleUrls: ['./product-list.component.scss'],
})
export class OCMProductList implements OnInit, OnDestroy {
  @Input() products: ListPage<HSMeProduct>
  @Input() shipFromSources: ShipFromSourcesDic
  @Input() isProductListLoading: boolean
  alive = true
  facets: ListFacet[]
  categoryCrumbs: Category[]
  favoriteProducts: string[] = []
  hasFilters = false
  showingFavoritesOnly = false
  closeIcon = faTimes
  numberOfItemsInPagination = 10
  searchTermForProducts = ''
  isAnon: boolean

  constructor(private context: ShopperContextService) {}

  ngOnInit(): void {
    this.context.productFilters.activeFiltersSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe(this.handleFiltersChange)
    this.context.currentUser.onChange((user) => {
      this.isAnon = this.context.currentUser.isAnonymous()
      this.favoriteProducts = user.FavoriteProductIDs
    })
    if (getScreenSizeBreakPoint() === 'xs') {
      this.numberOfItemsInPagination = 3
    } else if (getScreenSizeBreakPoint() === 'sm') {
      this.numberOfItemsInPagination = 4
    }
  }

  clearAllFilters(): void {
    this.context.productFilters.clearAllFilters()
  }

  changePage(page: number): void {
    this.context.productFilters.toPage(page)
  }

  toggleFilterByFavorites(): void {
    this.context.productFilters.filterByFavorites(!this.showingFavoritesOnly)
  }

  isFavorite(productID: string): boolean {
    return this.favoriteProducts.includes(productID)
  }

  setActiveCategory(categoryID: string): void {
    this.context.productFilters.filterByCategory(categoryID)
  }

  ngOnDestroy(): void {
    this.alive = false
  }

  private handleFiltersChange = (filters: ProductFilters): void => {
    this.showingFavoritesOnly = filters.showOnlyFavorites
    this.hasFilters = this.context.productFilters.hasFilters()
    this.categoryCrumbs = this.context.categories.breadCrumbs
    this.searchTermForProducts = filters.search
  }
}
