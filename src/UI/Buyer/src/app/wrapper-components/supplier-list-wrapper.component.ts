import { Component, OnInit, OnDestroy } from '@angular/core'
import { ActivatedRoute } from '@angular/router'
import { takeWhile } from 'rxjs/operators'
import { ListPage } from '@ordercloud/headstart-sdk'
import { Supplier } from 'ordercloud-javascript-sdk'
import { ShopperContextService } from '../services/shopper-context/shopper-context.service'
import { TempSdk } from '../services/temp-sdk/temp-sdk.service'
import { BuyerAppFilterType } from '../models/filter-config.types'

@Component({
  template: `
    <ocm-supplier-list
      [suppliers]="suppliers"
      [supplierFilterConfig]="supplierFilterConfig"
    ></ocm-supplier-list>
  `,
})
export class SupplierListWrapperComponent implements OnInit, OnDestroy {
  suppliers: ListPage<Supplier>
  supplierFilterConfig: any[]
  alive = true

  constructor(
    private activatedRoute: ActivatedRoute,
    public context: ShopperContextService,
    private tempSdk: TempSdk
  ) {}

  ngOnInit(): void {
    this.setUpData()
  }

  ngOnDestroy(): void {
    this.alive = false
  }

  private async setUpData(): Promise<void> {
    this.suppliers = this.activatedRoute.snapshot.data.products
    await this.getSupplierCategories()
    this.setSupplierCountryIfNeeded()
    this.setBuyerFilterIfNeeded()
    this.context.supplierFilters.activeFiltersSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe(this.handleFiltersChange)
  }

  private handleFiltersChange = async (): Promise<void> => {
    this.suppliers = await this.context.supplierFilters.listSuppliers()
  }

  private getSupplierCategories = async (): Promise<void> => {
    const supplierFilterConfigResponse = await this.tempSdk.getSupplierFilterConfig()
    this.supplierFilterConfig = supplierFilterConfigResponse.Items.map(
      (s) => s.Doc
    )
  }

  private setSupplierCountryIfNeeded(): void {
    if (
      this.supplierFilterConfig.find(
        (s) =>
          s.Path === 'xp.CountriesServicing' &&
          s.BuyerAppFilterType === BuyerAppFilterType.NonUI
      )
    ) {
      this.context.supplierFilters.setNonURLFilter(
        'xp.CountriesServicing',
        this.context.currentUser.get().xp.Country
      )
    }
  }

  private setBuyerFilterIfNeeded(): void {
    this.context.supplierFilters.setNonURLFilter(
      'xp.BuyersServicing',
      this.context.currentUser.get().Buyer.ID
    )
  }
}
