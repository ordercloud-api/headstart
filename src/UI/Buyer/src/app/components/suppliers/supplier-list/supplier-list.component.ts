import {
  Component,
  Input,
  OnChanges,
  ViewChild,
  OnDestroy,
} from '@angular/core'
import { Supplier, ListPage } from 'ordercloud-javascript-sdk'
import { faTimes, faFilter } from '@fortawesome/free-solid-svg-icons'
import { FormControl, FormGroup } from '@angular/forms'
import { NgbPopover } from '@ng-bootstrap/ng-bootstrap'
import { takeWhile } from 'rxjs/operators'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import {
  BuyerAppFilterType,
  SupplierFilters,
} from 'src/app/models/filter-config.types'

@Component({
  templateUrl: './supplier-list.component.html',
  styleUrls: ['./supplier-list.component.scss'],
})
export class OCMSupplierList implements OnChanges, OnDestroy {
  @Input() suppliers: ListPage<Supplier>
  _supplierFilterConfig: any[]
  @ViewChild('popover', { static: false }) public popover: NgbPopover
  alive = true
  searchTermForSuppliers: string = null
  filterForm: FormGroup
  faTimes = faTimes
  faFilter = faFilter
  serviceCategory = ''
  activeFilters = {}
  activeFilterCount = 0

  constructor(private context: ShopperContextService) {}

  @Input() set supplierFilterConfig(value: any[]) {
    this._supplierFilterConfig = value
    this.setForm()
    this.context.supplierFilters.activeFiltersSubject
      .pipe(takeWhile(() => this.alive))
      .subscribe(this.handleFiltersChange)
  }

  ngOnChanges(): void {
    this.activeFilterCount = Object.keys(
      this.context.supplierFilters.activeFiltersSubject.value.activeFilters
    ).length
  }

  setForm(): void {
    const formGroup = {}
    this._supplierFilterConfig.forEach((filterConfig) => {
      if (filterConfig.BuyerAppFilterType === BuyerAppFilterType.SelectOption) {
        formGroup[filterConfig.Path] = new FormControl('')
      }
    })
    this.filterForm = new FormGroup(formGroup)
  }

  searchSuppliers(searchStr: string): void {
    this.searchTermForSuppliers = searchStr
    this.context.supplierFilters.searchBy(searchStr)
  }

  changePage(page: number): void {
    this.context.supplierFilters.toPage(page)
    window.scrollTo(0, null)
  }

  applyFilters(): void {
    const filters = {}
    this._supplierFilterConfig.forEach((filterConfig) => {
      if (filterConfig.BuyerAppFilterType === BuyerAppFilterType.SelectOption)
        filters[filterConfig.Path] = this.filterForm.value[filterConfig.Path]
    })
    this.context.supplierFilters.filterByFields(filters)
    this.popover?.close()
  }

  clearFilters(): void {
    this.context.supplierFilters.clearAllFilters()
  }

  openPopover(): void {
    this.popover?.open()
  }

  closePopover(): void {
    this.popover?.close()
  }

  ngOnDestroy(): void {
    this.alive = false
  }

  hasFiltersAvailable(): boolean {
    if (this._supplierFilterConfig) {
      const options = this._supplierFilterConfig.filter(
        (filter) => filter.BuyerAppFilterType === 'SelectOption'
      )
      return options ? Boolean(options.length) : false
    }
    return false
  }

  private handleFiltersChange = (filters: SupplierFilters): void => {
    if (filters.activeFilters) {
      this.searchTermForSuppliers = filters.search || ''
      this._supplierFilterConfig.forEach((filterConfig) => {
        if (filterConfig.BuyerAppFilterType === BuyerAppFilterType.SelectOption)
          this.filterForm.controls[filterConfig.Path].setValue(
            filters.activeFilters[filterConfig.Path]
          )
      })
    }
  }
}
