import { Component, Input, Output, EventEmitter } from '@angular/core'
import { getPrimaryLineItemImage } from 'src/app/services/images.helpers'
import { Supplier } from 'ordercloud-javascript-sdk'
import { HSOrderReturn } from '@ordercloud/headstart-sdk';
import { HSLineItem } from '@ordercloud/headstart-sdk'
import { UntypedFormGroup } from '@angular/forms'
import { ReturnTranslations } from './models/return-translations.model'
import {
  returnHeaders,
  returnReasons,
} from './constants/return-table.constants'
import {
  CanReturn,
  NumberCanReturn,
  NumberHasReturned,
} from 'src/app/services/lineitem-status.helper'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './order-return-table.component.html',
  styleUrls: ['./order-return-table.component.scss'],
})
export class OCMOrderReturnTable {
  @Input() supplier: Supplier
  @Input() form: UntypedFormGroup
  @Input() lineItems: HSLineItem[]
  @Input() orderReturns: HSOrderReturn[]
  @Output() quantitiesToReturnEvent = new EventEmitter<number>()

  translationData: ReturnTranslations = {
    Headers: returnHeaders,
    AvailableReasons: returnReasons,
  }

  get rows(): UntypedFormGroup[] {
    // eslint-disable-next-line @typescript-eslint/no-unsafe-assignment
    const form = this.form as any
    // eslint-disable-next-line @typescript-eslint/no-unsafe-member-access
    return form.controls.lineItems.controls as UntypedFormGroup[]
  }

  constructor(private context: ShopperContextService) {}

  getImageUrl(lineItemID: string): string {
    return getPrimaryLineItemImage(
      lineItemID,
      this.lineItems,
      this.context.currentUser.get()
    )
  }

  toProductDetails(productID: string): void {
    this.context.router.toProductDetails(productID)
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

  isRowEnabled(row: UntypedFormGroup): boolean {
    return CanReturn(row.controls.lineItem.value, this.orderReturns)
  }

  isRowSelected(row: UntypedFormGroup): boolean {
    return row.controls.selected.value as boolean
  }

  getQuantityHasReturned(lineItem: HSLineItem): number {
    return NumberHasReturned(lineItem, this.orderReturns)
  }

  selectRow(row: UntypedFormGroup): void {
    row.controls.quantityToReturn.enable()
    row.controls.returnReason.enable()
    row.controls.selected.setValue(true)
  }

  deselectRow(row: UntypedFormGroup): void {
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

  toggle(row?: UntypedFormGroup): void {
    if (row.controls.selected.value) {
      // was deselected, now wants to select
      this.selectRow(row)
    } else {
      // was selected, now wants to deselect
      this.deselectRow(row)
    }
  }
}
