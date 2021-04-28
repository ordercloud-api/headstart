import { Component, OnInit, OnDestroy } from '@angular/core'
import { ActivatedRoute } from '@angular/router'
import { ShopperContextService } from '../services/shopper-context/shopper-context.service'
import { takeWhile } from 'rxjs/operators'
import { ListPage } from 'ordercloud-javascript-sdk'
import { isEmpty as _isEmpty, uniq as _uniq } from 'lodash'
import { SupplierFilterService } from '../services/supplier-filter/supplier-filter.service'
import { HSMeProduct } from '@ordercloud/headstart-sdk'
import { ShipFromSourcesDic } from '../models/shipping.types'

@Component({
  template: `
    <ocm-product-list
      *ngIf="products"
      [products]="products"
      [shipFromSources]="shipFromSources"
      [isProductListLoading]="isProductListLoading"
    ></ocm-product-list>
  `,
})
export class ProductListWrapperComponent implements OnInit, OnDestroy {
  products: ListPage<HSMeProduct>
  shipFromSources: ShipFromSourcesDic = {}
  alive = true
  isProductListLoading = true

  constructor(
    public context: ShopperContextService,
    private supplierFilterService: SupplierFilterService,
    private activatedRoute: ActivatedRoute
  ) {}

  ngOnInit(): void {
    if (_isEmpty(this.activatedRoute.snapshot.queryParams)) {
      this.context.categories.setActiveCategoryID(null)
    }
    this.context.productFilters.activeFiltersSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe(this.handleFiltersChange)
  }

  ngOnDestroy(): void {
    this.alive = false
  }

  private handleFiltersChange = async (): Promise<void> => {
    this.isProductListLoading = true
    const user = this.context.currentUser.get()
    const userGroups = user?.UserGroups?.length
    if (!userGroups) {
      throw new Error(
        'User is not configured - must be assigned to at least one location group'
      )
    }
    try {
      this.products = await this.context.productFilters.listProducts()
      const sourceIds = {}
      // gather supplier IDs and unique shipFromAddress IDs per supplier
      this.products.Items.forEach((p) => {
        if (!p.DefaultSupplierID || !p.ShipFromAddressID) return
        const source = sourceIds[p.DefaultSupplierID]
        if (!source) {
          sourceIds[p.DefaultSupplierID] = [p.ShipFromAddressID]
        } else {
          sourceIds[p.DefaultSupplierID] = _uniq([
            ...source,
            p.ShipFromAddressID,
          ])
        }
      })
      Object.keys(sourceIds).forEach((supplierId) => {
        sourceIds[supplierId].forEach(async (addressId) => {
          if (!this.shipFromSources[supplierId]) {
            this.shipFromSources[supplierId] = []
          }
          if (
            !this.shipFromSources[supplierId].length ||
            this.shipFromSources[supplierId]
              .map((address) => address.ID)
              .indexOf(addressId) < 0
          ) {
            const address = await this.supplierFilterService.getSupplierAddress(
              supplierId,
              addressId
            )
            this.shipFromSources[supplierId] = [
              ...this.shipFromSources[supplierId],
              address,
            ]
          }
        })
      })
    } finally {
      window.scroll(0, 0)
      this.isProductListLoading = false
    }
  }
}
