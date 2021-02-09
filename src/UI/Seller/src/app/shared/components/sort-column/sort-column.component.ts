import { Component, Input, Output, EventEmitter } from '@angular/core'
import { faCaretUp, faCaretDown } from '@fortawesome/free-solid-svg-icons'

@Component({
  selector: 'shared-sort-column',
  templateUrl: './sort-column.component.html',
  styleUrls: ['./sort-column.component.scss'],
})
export class SortColumnComponent {
  constructor() {}
  faCaretUp = faCaretUp
  faCaretDown = faCaretDown

  // The name of the field providing data to this column. For example, ID, Description, Active
  @Input() fieldName: string

  // Will be one of 'fieldName', '!fieldName', or undefined if not sorting on this column.
  @Input() currentSort: string

  // Will emit a string containing sort info in the OrderCloud API request syntax.
  // e.g. 'ID' means sort by ID accesending, '!ID' will sort descending.
  // Documentation - https://developer.ordercloud.io/documentation/platform-guides/basic-api-features/sorting
  @Output() sort = new EventEmitter<string>()

  changeSort() {
    let sort
    switch (this.currentSort) {
      case this.fieldName:
        sort = `!${this.fieldName}`
        break
      case `!${this.fieldName}`:
        // setting to undefined so sdk ignores parameter
        sort = undefined
        break
      default:
        sort = this.fieldName
    }
    this.sort.emit(sort)
  }

  showCaretIcon(): boolean {
    return (
      this.currentSort === this.fieldName ||
      this.currentSort === `!${this.fieldName}`
    )
  }
}
