import { Injectable } from '@angular/core'
import { BehaviorSubject } from 'rxjs'
import { Router, Params, ActivatedRoute } from '@angular/router'
import { transform as _transform, pickBy as _pickBy } from 'lodash'
import { Suppliers, Supplier, Sortable, Address, SupplierAddresses } from 'ordercloud-javascript-sdk'
import { ListPage } from '@ordercloud/headstart-sdk'
import { SupplierFilters } from 'src/app/models/filter-config.types'

// TODO - this service is only relevent if you're already on the product details page. How can we enforce/inidcate that?
@Injectable({
  providedIn: 'root',
})
export class SupplierFilterService {
  public activeFiltersSubject: BehaviorSubject<SupplierFilters> = new BehaviorSubject<SupplierFilters>(
    this.getDefaultParms()
  )
  public activeHiddenFilters: any = {}

  private readonly nonFilterQueryParams = ['page', 'sortBy', 'search']

  constructor(private router: Router, private activatedRoute: ActivatedRoute) {
    this.activatedRoute.queryParams.subscribe((params) => {
      if (this.router.url.startsWith('/suppliers')) {
        this.readFromUrlQueryParams(params)
      } else {
        this.activeFiltersSubject.next(this.getDefaultParms())
      }
    })
  }

  // Used to update the URL
  mapToUrlQueryParams(model: SupplierFilters): Params {
    const { page, sortBy, search, supplierID, activeFilters } = model
    return { page, sortBy, search, supplierID, ...activeFilters }
  }

  async listSuppliers(): Promise<ListPage<Supplier>> {
    const {
      page,
      sortBy,
      search,
      supplierID,
      activeFilters,
    } = this.activeFiltersSubject.value
    const allFilters = { ...activeFilters, ...this.activeHiddenFilters }
    return await Suppliers.List({
      page,
      search,
      sortBy: sortBy ? sortBy : ['Name'],
      filters: this.createFilters(allFilters, supplierID),
    })
  }

  async getSupplierAddress(supplierId, shipFromId): Promise<Address> {
    return await SupplierAddresses.Get(supplierId, shipFromId)
  }

  setNonURLFilter(key: string, value: string): void {
    this.activeHiddenFilters = { ...this.activeHiddenFilters, [key]: value }
  }

  toSupplier(supplierID: string): void {
    this.patchFilterState({
      supplierID: supplierID || undefined,
      page: undefined,
    })
  }

  toPage(pageNumber: number): void {
    this.patchFilterState({ page: pageNumber || undefined })
  }

  sortBy(field: Sortable<'Suppliers.List'>): void {
    this.patchFilterState({ sortBy: field || undefined, page: undefined })
  }

  filterByFields(filter: any): void {
    const activeFilters = this.activeFiltersSubject.value.activeFilters || {}
    const newActiveFilters = { ...activeFilters, ...filter }
    this.patchFilterState({ activeFilters: newActiveFilters, page: undefined })
  }

  addHiddenFilter(filter: any): void {}

  searchBy(searchTerm: string): void {
    this.patchFilterState({ search: searchTerm || undefined, page: undefined })
  }

  clearSort(): void {
    this.sortBy(undefined)
  }

  clearSearch(): void {
    this.searchBy(undefined)
  }

  clearAllFilters(): void {
    this.patchFilterState(this.getDefaultParms())
  }

  hasFilters(): boolean {
    const filters = this.activeFiltersSubject.value
    return Object.entries(filters).some(([key, value]) => !!value)
  }

  // Handle URL updates
  private readFromUrlQueryParams(params: Params): void {
    const { page, sortBy, search, supplierID } = params
    const activeFilters = _pickBy(
      params,
      (_value, _key) => !this.nonFilterQueryParams.includes(_key)
    )
    this.activeFiltersSubject.next({
      page,
      sortBy,
      search,
      supplierID,
      activeFilters,
    })
  }

  private getDefaultParms(): SupplierFilters {
    // default params are grabbed through a function that returns an anonymous object to avoid pass by reference bugs
    return {
      supplierID: undefined,
      page: undefined,
      sortBy: undefined,
      search: undefined,
      activeFilters: {},
    }
  }

  private createFilters(activeFilters: any, supplierID: string): any {
    const filters = _transform(
      activeFilters,
      (result, value, key: string) => (result[key.toLocaleLowerCase()] = value),
      {}
    ) as any
    filters.ID = supplierID || undefined
    return filters
  }

  private patchFilterState(patch: SupplierFilters): void {
    const activeFilters = { ...this.activeFiltersSubject.value, ...patch }
    const queryParams = this.mapToUrlQueryParams(activeFilters)
    this.router.navigate([], { queryParams }) // update url, which will call readFromUrlQueryParams()
  }
}
