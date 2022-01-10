import { Component, Input, OnChanges, SimpleChanges } from '@angular/core'
import * as moment from 'moment'

@Component({
  selector: 'reports-preview-component',
  templateUrl: './reports-preview.component.html',
  styleUrls: ['./reports-preview.component.scss'],
})
export class ReportsPreviewComponent implements OnChanges {
  @Input()
  reportData: object[]
  @Input()
  originalHeaders: string[]
  @Input()
  displayHeaders: string[]
  @Input()
  selectedTemplateID: string
  pipeName: string

  constructor() {}

  ngOnChanges(changes: SimpleChanges): void {
    if (changes.selectedTemplateID) {
      this.reportData = []
    }
  }

  formatData(item: any, header: string): string {
    if (header.includes('.')) {
      return this.getNestedValue(item, header)
    } else if (header.toLowerCase().includes('date')) {
      return moment(item[header]).format('MM/DD/YYYY')
    } else return item[header]
  }

  requiresPipe(header: string): boolean {
    if (header.toLowerCase().includes('phone')) {
      this.pipeName = 'phone'
      return true
    } else if (
      header.toLowerCase().includes('total') ||
      header.toLowerCase().includes('cost') ||
      header.toLowerCase().includes('discount')
    ) {
      this.pipeName = 'currency'
      return true
    } else if (header.toLowerCase().includes('percent')) {
      this.pipeName = 'percent'
      return true
    } else {
      return false
    }
  }

  getNestedValue(item: any, header: string): string {
    const props = header.split('.')
    let nestedValue = { ...item }
    for (let i = 0; i < props.length; i++) {
      if (Object.keys(nestedValue).includes(props[i])) {
        nestedValue = nestedValue[props[i]]
        if (nestedValue === (undefined || null)) return null
      } else {
        return null
      }
    }
    if (header.toLowerCase().includes('date')) {
      return moment(nestedValue).format('MM/DD/YYYY')
    }
    if (typeof nestedValue === 'boolean') return nestedValue.toString()
    return nestedValue
  }
}
