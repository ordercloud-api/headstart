import { Component, Input, Output, EventEmitter } from '@angular/core'
import { FormGroup } from '@angular/forms'

@Component({
  selector: 'reports-processing-component',
  templateUrl: './reports-processing.component.html',
  styleUrls: ['./reports-processing.component.scss'],
})
export class ReportsProcessingComponent {
  @Input()
  selectedTemplateID: string
  @Input()
  reportDownloading: boolean
  @Input()
  fetchingPreview: boolean
  @Input()
  reportSelectionForm: FormGroup
  @Input()
  adHocFilters: string[]
  @Output()
  handlePreviewReport = new EventEmitter<any>()
  @Output()
  handleDownloadReport = new EventEmitter<string>()

  constructor() {}

  previewReport(selectedTemplateID: string): void {
    const reportRequestBody = this.retrieveReportRequestBody(selectedTemplateID)
    this.handlePreviewReport.emit(reportRequestBody)
  }

  downloadReport(selectedTemplateID: string): void {
    const reportRequestBody = this.retrieveReportRequestBody(selectedTemplateID)
    this.handleDownloadReport.emit(reportRequestBody)
  }

  retrieveReportRequestBody(selectedTemplateID: string): any {
    const filterDictionary = new Object()
    if (this.adHocFilters?.length) {
      this.adHocFilters.forEach((filter) => {
        filterDictionary[filter] =
          this.reportSelectionForm.controls[filter].value
      })
    }
    const reportRequestBody = {
      selectedTemplateID,
      filterDictionary,
    }
    return reportRequestBody
  }
}
