import {
  Component,
  OnInit,
  Input,
  Output,
  EventEmitter,
  Inject,
} from '@angular/core'
import { ProductFacets, ProductFacet } from 'ordercloud-javascript-sdk'
import { faCheckCircle } from '@fortawesome/free-solid-svg-icons'
import { cloneDeep } from 'lodash'
import { applicationConfiguration } from '@app-seller/config/app.config'
import { HeadStartSDK, HSProduct } from '@ordercloud/headstart-sdk'
import { AppConfig } from '@app-seller/models/environment.types'

@Component({
  selector: 'product-filters-component',
  templateUrl: './product-filters.component.html',
  styleUrls: ['./product-filters.component.scss'],
})
export class ProductFilters implements OnInit {
  facetOptions: ProductFacet[]
  faCheckCircle = faCheckCircle
  sellerFilterOverride: boolean
  facetsOnProductStatic: any[]
  facetsOnProductEditable: any[]
  overriddenChanges: boolean
  savingOverriddenFilters: boolean

  @Input() set facetsOnProduct(facets: any[]) {
    this.facetsOnProductStatic = cloneDeep(facets)
    this.facetsOnProductEditable = facets
  }
  @Input() readonly = false
  @Input() superProduct: HSProduct
  @Output() updatedFacets = new EventEmitter<any>()

  constructor(@Inject(applicationConfiguration) private appConfig: AppConfig) {}

  ngOnInit(): void {
    this.getFacets()
  }

  async getFacets(): Promise<void> {
    const facetsListPage = await ProductFacets.List({ pageSize: 100 })
    let facets = facetsListPage.Items
    if (facetsListPage.Meta.TotalPages > 1) {
      for (let i = 2; i <= facetsListPage.Meta.TotalPages; i++) {
        const additionalFacets = await ProductFacets.List({
          pageSize: 100,
          page: i,
        })
        facets = facets.concat(additionalFacets.Items)
      }
    }
    this.facetOptions = facets.filter((f) => f?.xp?.Options?.length)
  }

  areFacetOptionsSelected(facet: ProductFacet): boolean {
    const productXpFacetKey = facet?.XpPath?.split('.')[1]
    return (
      Object.keys(this.facetsOnProductEditable).includes(productXpFacetKey) &&
      this.facetsOnProductEditable[productXpFacetKey].length
    )
  }

  isFacetOptionApplied(facet: ProductFacet, option: string): boolean {
    const productXpFacetKey = facet?.XpPath?.split('.')[1]
    const facetOptionsOnProduct =
      this.facetsOnProductEditable[productXpFacetKey]
    const isFacetOptionApplied =
      facetOptionsOnProduct && facetOptionsOnProduct.includes(option)
    return isFacetOptionApplied
  }

  toggleFacetOption(facet: ProductFacet, option: string): void {
    const productXpFacetKey = facet?.XpPath?.split('.')[1]
    let facetOnXp = this.facetsOnProductEditable[productXpFacetKey]
    delete this.facetsOnProductEditable[productXpFacetKey]
    if (!facetOnXp) {
      facetOnXp = []
    }
    if (facetOnXp.includes(option)) {
      facetOnXp = facetOnXp.filter((o) => o !== option)
      this.facetsOnProductEditable = {
        ...this.facetsOnProductEditable,
        [productXpFacetKey]: facetOnXp,
      }
    } else {
      facetOnXp.push(option)
    }
    if (facetOnXp.length > 0) {
      this.facetsOnProductEditable = {
        ...this.facetsOnProductEditable,
        [productXpFacetKey]: facetOnXp,
      }
    }
    if (!this.readonly) {
      this.updatedFacets.emit(this.facetsOnProductEditable)
    } else {
      this.overrideFacets()
    }
  }

  overrideFacets(): void {
    if (this.checkForFacetOverrides()) {
      this.overriddenChanges = true
    } else {
      this.overriddenChanges = false
    }
  }

  toggleSellerFilterOverride(): void {
    this.overriddenChanges = false
    this.sellerFilterOverride = !this.sellerFilterOverride
    if (!this.sellerFilterOverride) {
      this.facetsOnProductEditable = cloneDeep(this.facetsOnProductStatic)
    }
  }

  checkForFacetOverrides(): boolean {
    const keys = Object.keys(this.facetsOnProductEditable)
    let changeDetected = false
    keys.forEach((key) => {
      if (
        this.facetsOnProductEditable[key]?.length !==
          this.facetsOnProductStatic[key]?.length ||
        !this.facetsOnProductEditable[key].every((item) =>
          this.facetsOnProductStatic[key].includes(item)
        ) ||
        !(key in this.facetsOnProductStatic)
      ) {
        changeDetected = true
      }
    })
    return changeDetected
  }

  async saveFilterOverrides(): Promise<void> {
    this.savingOverriddenFilters = true
    ;(this.superProduct.xp as any).Facets = this.facetsOnProductEditable
    const product = await HeadStartSDK.Products.FilterOptionOverride(
      this.superProduct.ID,
      this.superProduct
    )
    this.superProduct = product
    this.facetsOnProductStatic = cloneDeep(product.xp.Facets)
    this.facetsOnProductEditable = cloneDeep(product.xp.Facets)
    this.sellerFilterOverride = false
    this.savingOverriddenFilters = false
  }
}
