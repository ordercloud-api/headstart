import { Component, Input, Output, EventEmitter } from '@angular/core'
import { HSBuyer, HSSupplier, ReportTemplate } from '@ordercloud/headstart-sdk'
import { FormGroup } from '@angular/forms'

@Component({
  selector: 'reports-selection-component',
  templateUrl: './reports-selection.component.html',
  styleUrls: ['./reports-selection.component.scss'],
})
export class ReportsSelectionComponent {
  @Input()
  reportTypes: any[]
  @Input()
  reportTemplates: ReportTemplate[]
  @Input()
  reportSelectionForm: FormGroup
  @Input()
  displayHeaders: string[]
  @Input()
  adHocFilters: string[]
  @Input()
  suppliers: HSSupplier[]
  @Input()
  buyers: HSBuyer[]
  @Output()
  handleReportTypeSelection = new EventEmitter<string>()
  @Output()
  handleReportTemplateSelection = new EventEmitter<string>()
  @Output()
  handleReportAdHocFiltersSelection = new EventEmitter<any>()
  selectedTemplate: ReportTemplate = {}
  showDetails = false
  filterEntries: string[][]

  constructor() {}

  updateReportType(event: string): void {
    this.handleReportTypeSelection.emit(event)
  }

  updateReportTemplate(event: string): void {
    this.handleReportTemplateSelection.emit(event)
    this.selectedTemplate = this.reportTemplates.find(
      (template) => template.TemplateID === event
    )
    this.filterEntries = Object.entries(this.selectedTemplate.Filters)
  }

  updateReportAdHocFilters(event: string, filter: string): void {
    const filterSelection = { event, filter }
    this.handleReportAdHocFiltersSelection.emit(filterSelection)
  }

  toggleShowDetails(): void {
    this.showDetails = !this.showDetails
  }

  getDetailsDisplayVerb(): string {
    return this.showDetails ? 'Hide' : 'Show'
  }

  getFilterType(filter: string): string {
    if (filter.includes('Date')) return 'date'
    if (filter.includes('Time')) return 'time'
  }

  getFilterNameDisplay(filter: string): string {
    return (
      filter.match(/[A-Z][a-z]+|[0-9]+/g).join(' ') +
      (filter.includes('Time') ? ' (Optional)' : ' (Required)')
    )
  }
}
