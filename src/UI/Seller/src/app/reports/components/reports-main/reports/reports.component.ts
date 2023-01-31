import { Component, OnInit } from '@angular/core'
import {
  UntypedFormGroup,
  UntypedFormControl,
  Validators,
} from '@angular/forms'
import {
  HeadStartSDK,
  HSBuyer,
  HSSupplier,
  ReportTemplate,
  ReportType,
} from '@ordercloud/headstart-sdk'
import { ReportsTypeService } from '@app-seller/shared/services/middleware-api/reports-type.service'
import {
  buyerLocation,
  salesOrderDetail,
  purchaseOrderDetail,
  lineItemDetail,
  shipmentDetail,
  productDetail,
} from '../../reports-template/models/headers'
import { Suppliers } from 'ordercloud-javascript-sdk'
import { flatten as _flatten } from 'lodash'
import { AppAuthService } from '@app-seller/auth/services/app-auth.service'
import { SELLER } from '@app-seller/models/user.types'

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss'],
})
export class ReportsComponent implements OnInit {
  reportSelectionForm: UntypedFormGroup
  reportTypes: any
  reportTemplates: ReportTemplate[] = []
  selectedTemplateID: string
  selectedTemplate: ReportTemplate
  selectedReportType: string
  reportData: object[]
  displayHeaders: string[]
  adHocFilters: string[]
  fetchingPreview = false
  reportDownloading = false
  suppliers: HSSupplier[] = []
  buyers: HSBuyer[] = []
  isSellerUser = false

  constructor(
    private reportsTypeService: ReportsTypeService,
    private appAuthService: AppAuthService
  ) {
    this.isSellerUser = this.appAuthService.getOrdercloudUserType() === SELLER
  }

  async ngOnInit(): Promise<void> {
    const reportTypes = await this.reportsTypeService.list()
    this.reportTypes = reportTypes.Items
    this.createReportSelectionForm()
  }

  createReportSelectionForm(): void {
    this.reportSelectionForm = new UntypedFormGroup({
      ReportType: new UntypedFormControl(null, Validators.required),
      ReportTemplate: new UntypedFormControl(null, Validators.required),
    })
  }

  async handleReportTypeSelection(event: string): Promise<void> {
    this.resetForm()
    this.selectedReportType = event
    this.reportSelectionForm.controls.ReportType.setValue(
      this.selectedReportType
    )
    this.adHocFilters = this.setAdHocFilters(this.selectedReportType)
    if (this.adHocFilters?.length) {
      this.adHocFilters.forEach(async (filter) => {
        this.reportSelectionForm.addControl(
          filter,
          new UntypedFormControl(null)
        )
        if (filter.includes('Date')) {
          this.reportSelectionForm.controls[filter].setValidators(
            Validators.required
          )
        }
        if (filter.includes('SupplierID') && this.isSellerUser) {
          this.suppliers = await this.getAllSuppliers()
        }
        if (filter.includes('BrandID')) {
          this.buyers = await this.getAllBuyers()
        }
      })
    }
    this.reportTemplates =
      await HeadStartSDK.Reports.ListReportTemplatesByReportType(
        this.selectedReportType as ReportType
      )
  }

  resetForm(): void {
    this.createReportSelectionForm()
    this.reportData = []
    this.selectedTemplate = {}
  }

  setAdHocFilters(reportType: string): string[] {
    let adHocFilters = this.reportTypes.find(
      (type) => type.Value === reportType
    ).AdHocFilters
    if (!this.isSellerUser && adHocFilters) {
      adHocFilters = adHocFilters.filter((item) => item !== 'SupplierID')
    }
    return adHocFilters
  }

  handleReportTemplateSelection(event: string): void {
    this.selectedTemplateID = event
    this.selectedTemplate = this.reportTemplates.find(
      (template) => template.TemplateID === event
    )
    const headers = this.selectedTemplate.Headers
    this.displayHeaders = headers.map((header) =>
      this.fetchDisplayHeader(header)
    )
    this.reportSelectionForm.controls.ReportTemplate.setValue(event)
  }

  handleReportAdHocFiltersSelection(event: any): void {
    if (
      this.reportSelectionForm.contains(event.filter) &&
      event.event == 'null'
    ) {
      this.reportSelectionForm.controls[event.filter].reset()
    } else {
      this.reportSelectionForm.controls[event.filter].setValue(event.event)
    }
  }

  async handlePreviewReport(reportRequestBody: any): Promise<void> {
    this.fetchingPreview = true
    this.reportData = await HeadStartSDK.Reports.PreviewReport(
      this.selectedTemplate.ReportType,
      this.selectedTemplate.TemplateID
    )
    this.fetchingPreview = false
  }

  async handleDownloadReport(request: any): Promise<void> {
    this.reportDownloading = true
    await HeadStartSDK.Reports.DownloadReport(
      this.selectedTemplate.ReportType,
      this.selectedTemplate.TemplateID
    )
    this.reportDownloading = false
  }

  fetchDisplayHeader(header: string): string {
    let column
    switch (this.selectedReportType) {
      case 'BuyerLocation':
        column = buyerLocation.find((c) => c.path === header)
        return column?.value
      case 'SalesOrderDetail':
        column = salesOrderDetail.find((c) => c.path === header)
        return column?.value
      case 'PurchaseOrderDetail':
        column = purchaseOrderDetail.find((c) => c.path === header)
        return column?.value
      case 'LineItemDetail':
        column = lineItemDetail.find((c) => c.path === header)
        return column?.value
      case 'ProductDetail':
        column = productDetail.find((c) => c.path === header)
        return column?.value
      case 'ShipmentDetail':
        column = shipmentDetail.find((c) => c.path === header)
        return column?.value
    }
  }

  private async getAllSuppliers(): Promise<HSSupplier[]> {
    let suppliers: HSSupplier[] = []
    const listOptions = {
      page: 1,
      pageSize: 100,
      sortBy: ['Name'] as any,
    }
    const suppliersResponse = await Suppliers.List(listOptions)
    suppliers = [...suppliers, ...(suppliersResponse.Items as HSSupplier[])]
    if (suppliersResponse.Meta.TotalPages <= 1) {
      return suppliers
    } else {
      let supplierRequests = []
      for (let page = 2; page <= suppliersResponse.Meta.TotalPages; page++) {
        listOptions.page = page
        supplierRequests = [...supplierRequests, Suppliers.List(listOptions)]
      }
      return await Promise.all(supplierRequests).then((response) => {
        suppliers = [...suppliers, ..._flatten(response.map((r) => r.Items))]
        return suppliers
      })
    }
  }

  private async getAllBuyers(): Promise<HSBuyer[]> {
    return await this.reportsTypeService.getBuyerFilterValues()
  }
}
