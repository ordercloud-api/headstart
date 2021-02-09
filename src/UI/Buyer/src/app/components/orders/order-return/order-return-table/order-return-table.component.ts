import { Component, Input, Output, EventEmitter } from '@angular/core'
import { getPrimaryLineItemImage } from 'src/app/services/images.helpers'
import { MatTableDataSource } from '@angular/material/table'
import { SelectionModel } from '@angular/cdk/collections'
import { Supplier } from 'ordercloud-javascript-sdk'
import { HSLineItem } from '@ordercloud/headstart-sdk'
import { FormGroup, FormArray } from '@angular/forms'
import { CancelReturnTranslations } from './models/cancel-return-translations.model'
import {
  returnHeaders,
  returnReasons,
  cancelReasons,
  cancelHeaders,
} from './constants/cancel-return-table.constants'
import { CancelReturnReason } from './models/cancel-return-translations.enum'
import {
  CanReturnOrCancel,
  NumberCanCancelOrReturn,
} from 'src/app/services/lineitem-status.helper'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './order-return-table.component.html',
  styleUrls: ['./order-return-table.component.scss'],
})
export class OCMOrderReturnTable {
  dataSource = new MatTableDataSource<any>([])
  selection = new SelectionModel<FormGroup>(true, [])
  _liGroup: HSLineItem[]
  quantitiesToReturn: number[] = []
  translationData: CancelReturnTranslations
  lineItems: FormArray
  columnsToDisplay: string[] = [
    'select',
    'product',
    'id',
    'price',
    'quantityOrdered',
    'quantityReturned',
    'quantityToReturnOrCancel',
    'returnReason',
  ]
  _action: string

  @Input() set liGroup(value: HSLineItem[]) {
    this._liGroup = value
  }
  @Input() supplier: Supplier
  @Input() set liGroupForm(value: FormGroup) {
    this.lineItems = value.controls.lineItems as FormArray
    this.dataSource = new MatTableDataSource<any>(this.lineItems.controls)
  }
  @Input() set action(value: string) {
    this._action = value
    if (value === 'return') {
      this.translationData = {
        Headers: returnHeaders,
        AvailableReasons: returnReasons,
      }
    } else {
      this.translationData = {
        Headers: cancelHeaders,
        AvailableReasons: cancelReasons,
      }
    }
  }
  @Output()
  quantitiesToReturnEvent = new EventEmitter<number>()

  constructor(private context: ShopperContextService) {}

  getImageUrl(lineItemID: string): string {
    return getPrimaryLineItemImage(
      lineItemID,
      this._liGroup,
      this.context.currentUser.get()
    )
  }

  toProductDetails(productID: string): void {
    this.context.router.toProductDetails(productID)
  }

  getReasonCode(reason: CancelReturnReason): string {
    const reasonCode = Object.keys(CancelReturnReason).find(
      (key) => CancelReturnReason[key] === reason
    )
    return reasonCode
  }

  /** Whether the number of selected elements matches the total number of enabled rows. */
  isAllEnabledSelected(): boolean {
    let numEnabledRows = 0
    let numSelectedRows = 0
    this.dataSource.data.forEach((row) => {
      if (this.isRowEnabled(row)) {
        numEnabledRows++
      }
      if (this.selection.isSelected(row)) {
        numSelectedRows++
      }
    })
    return numEnabledRows === numSelectedRows
  }

  isRowEnabled(row: FormGroup): boolean {
    return CanReturnOrCancel(row.controls.lineItem.value, this._action)
  }

  getQuantityReturnedCanceled(lineItem: HSLineItem): number {
    return NumberCanCancelOrReturn(lineItem, this._action)
  }

  selectRow(row: FormGroup): void {
    this.selection.select(row)
    row.controls.quantityToReturnOrCancel.enable()
    row.controls.returnReason.enable()
    row.controls.selected.setValue(true)
  }

  deselectRow(row: FormGroup): void {
    this.selection.deselect(row)
    row.controls.quantityToReturnOrCancel.disable()
    row.controls.returnReason.disable()
    row.controls.selected.setValue(false)
  }

  /** Selects all rows if they are not all selected; otherwise clear selection. */
  masterToggle(): void {
    if (this.isAllEnabledSelected()) {
      this.dataSource.data.forEach((row) => {
        if (this.isRowEnabled(row)) {
          this.deselectRow(row)
        }
      })
    } else {
      this.dataSource.data.forEach((row) => {
        if (this.isRowEnabled(row)) {
          this.selectRow(row)
        }
      })
    }
  }

  toggle(row?: FormGroup): void {
    if (this.selection.isSelected(row)) {
      this.deselectRow(row)
    } else {
      this.selectRow(row)
    }
  }

  /** The label for the checkbox on the passed row */
  checkboxLabel(i: number, row?: FormGroup): string {
    if (!row) {
      return `${this.isAllEnabledSelected() ? 'select' : 'deselect'} all`
    }
    return `${this.selection.isSelected(row) ? 'deselect' : 'select'} row ${i}`
  }
}
