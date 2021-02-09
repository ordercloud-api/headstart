import { Component, OnInit } from '@angular/core'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { FormGroup, FormControl, Validators } from '@angular/forms'
import { ReportsTemplateService } from '@app-seller/shared/services/middleware-api/reports-template.service'
import { ReportTemplate } from '@ordercloud/headstart-sdk'
import { ReportsTypeService } from '@app-seller/shared/services/middleware-api/reports-type.service'
import {
  buyerLocation,
  salesOrderDetail,
  purchaseOrderDetail,
  lineItemDetail,
} from '../../reports-template/models/headers'

@Component({
  selector: 'app-reports',
  templateUrl: './reports.component.html',
  styleUrls: ['./reports.component.scss'],
})
export class ReportsComponent implements OnInit {
  reportSelectionForm: FormGroup
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

  constructor(
    private currentUserService: CurrentUserService,
    private reportsTemplateService: ReportsTemplateService,
    private reportsTypeService: ReportsTypeService
  ) {}

  async ngOnInit(): Promise<void> {
    const reportTypes = await this.reportsTypeService.list()
    this.reportTypes = reportTypes.Items
    this.createReportSelectionForm()
  }

  createReportSelectionForm(): void {
    this.reportSelectionForm = new FormGroup({
      ReportType: new FormControl(null, Validators.required),
      ReportTemplate: new FormControl(null, Validators.required),
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
      this.adHocFilters.forEach((filter) => {
        this.reportSelectionForm.addControl(filter, new FormControl(null))
        if (filter.includes('Date')) {
          this.reportSelectionForm.controls[filter].setValidators(
            Validators.required
          )
        }
      })
    }
    this.reportTemplates = await this.reportsTemplateService.listReportTemplatesByReportType(
      this.selectedReportType
    )
  }

  resetForm(): void {
    this.createReportSelectionForm()
    this.reportData = []
    this.selectedTemplate = {}
  }

  setAdHocFilters(reportType: string): string[] {
    return this.reportTypes.find((type) => type.Value === reportType)
      .AdHocFilters
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
    this.reportSelectionForm.controls[event.filter].setValue(event.event)
  }

  async handlePreviewReport(reportRequestBody: any): Promise<void> {
    this.fetchingPreview = true
    this.reportData = await this.reportsTemplateService.previewReport(
      this.selectedTemplate,
      reportRequestBody
    )
    this.fetchingPreview = false
  }

  async handleDownloadReport(reportRequestBody: any): Promise<void> {
    this.reportDownloading = true
    await this.reportsTemplateService.downloadReport(
      this.selectedTemplate,
      reportRequestBody
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
    }
  }
}
