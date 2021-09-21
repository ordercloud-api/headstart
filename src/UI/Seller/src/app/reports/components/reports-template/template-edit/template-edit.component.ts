import {
  Component,
  Input,
  Output,
  EventEmitter,
  OnChanges,
  SimpleChanges,
} from '@angular/core'
import { ReportsTemplateService } from '@app-seller/shared/services/middleware-api/reports-template.service'
import { FormGroup } from '@angular/forms'
import {
  buyerLocation as buyerLocationHeaders,
  salesOrderDetail as salesOrderDetailHeaders,
  purchaseOrderDetail as purchaseOrderDetailHeaders,
  lineItemDetail as lineItemDetailHeaders,
  rmaDetail as rmaDetailHeaders,
  productDetail as productDetailHeaders,
  shipmentDetail as shipmentDetailHeaders,
} from '../models/headers'
import {
  buyerLocation as buyerLocationFilters,
  salesOrderDetail as salesOrderDetailFilters,
  purchaseOrderDetail as purchaseOrderDetailFilters,
  lineItemDetail as lineItemDetailFilters,
  rmaDetail as rmaDetailFilters,
  productDetail as productDetailFilters,
  shipmentDetail as shipmentDetailFilters,
} from '../models/filters'
import { faCheckCircle } from '@fortawesome/free-solid-svg-icons'
import { OcBuyerService, OcSupplierService } from '@ordercloud/angular-sdk'
import { cloneDeep } from 'lodash'
import { GeographyConfig } from '@app-seller/shared/models/supported-countries.constant'
import {
  AppGeographyService,
  FilterObject,
  OrderType,
  RMAType,
} from '@app-seller/shared'
import { ReportTemplate } from '@ordercloud/headstart-sdk'

@Component({
  selector: 'template-edit-component',
  templateUrl: './template-edit.component.html',
  styleUrls: ['./template-edit.component.scss'],
})
export class TemplateEditComponent implements OnChanges {
  @Input()
  resourceForm: FormGroup
  @Input()
  reportType: string
  @Input()
  updatedResource: ReportTemplate
  @Input()
  set resourceInSelection(template: ReportTemplate) {
    this.reportTemplate = template
    this.setHeadersAndFilters(this.reportType)
    if (this.reportsTemplateService.checkIfCreatingNew()) {
      this.isCreatingNew = true
      this.resourceForm.controls['ReportType'].setValue(this.reportType)
      this.updateResource.emit({
        field: 'ReportType',
        value: this.reportType,
        form: this.resourceForm,
      })
      this.handleSelectAllHeaders()
    }
  }
  @Output()
  updateResource = new EventEmitter<any>()
  reportTemplate: ReportTemplate
  isCreatingNew: boolean
  reportTemplateEditable: ReportTemplate
  headers: any[]
  filters: FilterObject[]
  filterChipsToDisplay: FilterObject[] = []
  faCheckCircle = faCheckCircle

  constructor(
    private reportsTemplateService: ReportsTemplateService,
    private geographyService: AppGeographyService,
    private supplierService: OcSupplierService,
    private ocBuyerService: OcBuyerService // DO NOT REMOVE THIS, its dynamically called at runtime
  ) {}

  ngOnChanges(changes: SimpleChanges) {
    if (
      changes.resourceInSelection?.currentValue !==
      changes.resourceInSelection?.previousValue
    ) {
      if (!this.reportsTemplateService.checkIfCreatingNew()) {
        this.filters?.forEach((filter) => {
          if (
            this.updatedResource.Filters &&
            this.updatedResource.Filters[filter.path]?.length
          ) {
            this.filterChipsToDisplay.push(filter)
          }
        })
      }
    }
  }

  handleUpdateReportTemplate(event: any, field: string): void {
    const value = ['AvailableToSuppliers'].includes(field)
      ? event.target.checked
      : event.target.value
    this.updateResource.emit({
      field: field,
      value: value,
      form: this.resourceForm,
    })
  }

  setHeadersAndFilters(reportType: string): void {
    switch (reportType) {
      case 'BuyerLocation':
        this.headers = buyerLocationHeaders
        this.filters = buyerLocationFilters
        break
      case 'SalesOrderDetail':
        this.headers = salesOrderDetailHeaders
        this.filters = salesOrderDetailFilters
        break
      case 'PurchaseOrderDetail':
        this.headers = purchaseOrderDetailHeaders
        this.filters = purchaseOrderDetailFilters
        break
      case 'LineItemDetail':
        this.headers = lineItemDetailHeaders
        this.filters = lineItemDetailFilters
        break
      case 'ProductDetail':
        this.headers = productDetailHeaders
        this.filters = productDetailFilters
        break
      case 'RMADetail':
        this.headers = rmaDetailHeaders
        this.filters = rmaDetailFilters
        break
      case 'ShipmentDetail':
        this.headers = shipmentDetailHeaders
        this.filters = shipmentDetailFilters
    }
    if (this.filters?.length) {
      this.populateFilters()
    }
  }

  async populateFilters(): Promise<void> {
    for (const filter of this.filters) {
      switch (filter.sourceType) {
        case 'oc':
          const data = await this[filter.source].List().toPromise()
          filter.filterValues = data.Items
          break
        case 'model':
          if (filter.name === 'Country') {
            filter.filterValues = GeographyConfig.getCountries()
          }
          if (filter.name === 'State') {
            // For now, filtering is setup only for US states.  Will eventually need a way to get all states relevant to Seller organization.
            filter.filterValues = this.geographyService.getStates('US')
          }
          if (filter.name === 'Order Type') {
            filter.filterValues = Object.values(OrderType)
          }
          if (filter.name === 'Submitted Order Status') {
            filter.filterValues = ['Open', 'Completed', 'Canceled']
          }
          if (filter.name === 'RMA Type') {
            filter.filterValues = Object.values(RMAType)
          }
          if (filter.name === 'RMA Status') {
            filter.filterValues = [
              'Requested',
              'Processing',
              'Approved',
              'Complete',
              'Denied',
            ]
          }
          if (filter.name === 'Shipping Status') {
            filter.filterValues = [
              'Shipped',
              'PartiallyShipped',
              'Canceled',
              'Processing',
              'Backordered',
            ]
          }
          if (filter.name === 'Product Status') {
            filter.filterValues = ['Draft', 'Published']
          }
          if (filter.name === 'Product Vendor') {
            const supplierList = await this.supplierService
              .List()
              .subscribe((result) => {
                filter.filterValues = result.Items
              })
          }
      }
    }
  }

  isHeaderSelected(header: string): boolean {
    return this.updatedResource.Headers?.includes(header)
  }

  isFilterValueSelected(filter: FilterObject, filterValue: any): boolean {
    return this.updatedResource.Filters[filter?.path]?.includes(
      filter.dataKey ? filterValue[filter.dataKey] : filterValue
    )
  }

  toggleHeader(selectedHeader: string): void {
    const headers = [...this.updatedResource.Headers]
    const i = headers?.indexOf(selectedHeader)
    if (i > -1) {
      headers?.splice(i, 1)
    } else {
      headers?.push(selectedHeader)
    }
    const headersToCompare = this.headers.map((header) => header.path)
    headers?.sort(
      (a, b) => headersToCompare.indexOf(a) - headersToCompare.indexOf(b)
    )
    this.resourceForm.controls.Headers.setValue(headers)
    this.updateResource.emit({
      field: 'Headers',
      value: headers,
      form: this.resourceForm,
    })
  }

  toggleFilter(filter: FilterObject, filterValue: any): void {
    const filters = cloneDeep(this.updatedResource.Filters)
    const selectedFilterValues = filters[filter.path]
    if (!selectedFilterValues || !selectedFilterValues.length) {
      filters[filter.path] = filter.dataKey
        ? [filterValue[filter.dataKey]]
        : [filterValue]
    } else {
      const selectedValue = filter.dataKey
        ? filterValue[filter.dataKey]
        : filterValue
      const i = selectedFilterValues.indexOf(selectedValue)
      if (i > -1) {
        selectedFilterValues.splice(i, 1)
      } else {
        selectedFilterValues.push(selectedValue)
      }
    }
    selectedFilterValues?.sort()
    this.resourceForm.controls.Filters.setValue(filters)
    this.updateResource.emit({
      field: 'Filters',
      value: filters,
      form: this.resourceForm,
    })
  }

  toggleIncludeAllValues(includeAll: boolean, filter: FilterObject): void {
    if (includeAll) {
      const i = this.filterChipsToDisplay.indexOf(filter)
      this.filterChipsToDisplay = this.filterChipsToDisplay.filter(
        (chipGrouping) => chipGrouping !== this.filterChipsToDisplay[i]
      )
      this.updatedResource.Filters[filter.path] = []
      this.resourceForm.controls.Filters.setValue(this.updatedResource.Filters)
      this.updateResource.emit({
        field: 'Filters',
        value: this.updatedResource.Filters,
        form: this.resourceForm,
      })
    } else {
      this.filterChipsToDisplay.push(filter)
      this.updatedResource.Filters[filter.path] = []
      this.resourceForm.controls.Filters.setValue(this.updatedResource.Filters)
      this.updateResource.emit({
        field: 'Filters',
        value: this.updatedResource.Filters,
        form: this.resourceForm,
      })
    }
  }

  checkForChipDisplay(filter: FilterObject): boolean {
    return this.filterChipsToDisplay.includes(filter)
  }

  handleSelectAllHeaders(): void {
    const headersPaths = this.headers.map((header) => header.path)
    this.resourceForm.controls.Headers.setValue(headersPaths)
    this.updateResource.emit({
      field: 'Headers',
      value: headersPaths,
      form: this.resourceForm,
    })
  }

  handleUnselectAllHeaders(): void {
    this.updatedResource.Headers = []
    this.resourceForm.controls.Headers.setValue(this.updatedResource.Headers)
    this.updateResource.emit({
      field: 'Headers',
      value: this.updatedResource.Headers,
      form: this.resourceForm,
    })
  }
}
