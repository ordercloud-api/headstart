import { Component, Input, Output, EventEmitter } from '@angular/core'
import { Supplier } from 'ordercloud-javascript-sdk'
import { HSOrderReturn } from '@ordercloud/headstart-sdk'
import { HSLineItem } from '@ordercloud/headstart-sdk'
import { FormGroup } from '@angular/forms'
import { ReturnTranslations } from './models/return-translations.model'
import {
  returnHeaders,
  returnReasons,
} from './constants/return-table.constants'
import { getPrimaryLineItemImage } from '@app-seller/shared/services/assets/asset.helper'
import {
  CanReturn,
  NumberCanReturn,
  NumberHasReturned,
} from '@app-seller/orders/line-item-status.helper'

@Component({
  selector: 'order-return-request-table',
  templateUrl: './order-return-request-table.component.html',
  styleUrls: ['./order-return-request-table.component.scss'],
})
export class OrderReturnRequestTable {
  @Input() supplier: Supplier
  @Input() form: FormGroup
  @Input() lineItems: HSLineItem[]
  @Input() orderReturns: HSOrderReturn[]
  @Output() quantitiesToReturnEvent = new EventEmitter<number>()

  translationData: ReturnTranslations = {
    Headers: returnHeaders,
    AvailableReasons: returnReasons,
  }

  get rows(): FormGroup[] {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
    const form = this.form as any
    // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
    return form.controls.lineItems.controls as FormGroup[]
  }

  getImageUrl(lineItemID: string): string {
    return getPrimaryLineItemImage(lineItemID, this.lineItems)
  }

  /** Whether the number of selected elements matches the total number of enabled rows. */
  isAllEnabledSelected(): boolean {
    let numEnabledRows = 0
    let numSelectedRows = 0
    this.rows.forEach((row) => {
      if (this.isRowEnabled(row)) {
        numEnabledRows++
      }
      if (this.isRowSelected(row)) {
        numSelectedRows++
      }
    })
    return numEnabledRows === numSelectedRows
  }

  isRowEnabled(row: FormGroup): boolean {
    return CanReturn(row.controls.lineItem.value, this.orderReturns)
  }

  isRowSelected(row: FormGroup): boolean {
    return row.controls.selected.value as boolean
  }

  getQuantityHasReturned(lineItem: HSLineItem): number {
    return NumberHasReturned(lineItem, this.orderReturns)
  }

  selectRow(row: FormGroup): void {
    row.controls.quantityToReturn.enable()
    row.controls.returnReason.enable()
    row.controls.selected.setValue(true)
  }

  deselectRow(row: FormGroup): void {
    row.controls.quantityToReturn.disable()
    row.controls.returnReason.disable()
    row.controls.selected.setValue(false)
  }

  /** Selects all rows if they are not all selected; otherwise clear selection. */
  masterToggle(): void {
    if (this.isAllEnabledSelected()) {
      this.rows.forEach((row) => {
        if (this.isRowEnabled(row)) {
          this.deselectRow(row)
        }
      })
    } else {
      this.rows.forEach((row) => {
        if (this.isRowEnabled(row)) {
          this.selectRow(row)
        }
      })
    }
  }

  toggle(row?: FormGroup): void {
    if (row.controls.selected.value) {
      // was deselected, now wants to select
      this.selectRow(row)
    } else {
      // was selected, now wants to deselect
      this.deselectRow(row)
    }
  }
}
