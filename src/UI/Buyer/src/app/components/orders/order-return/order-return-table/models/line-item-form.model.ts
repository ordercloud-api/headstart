import { FormControl, Validators } from '@angular/forms'
import { HSLineItem } from '@ordercloud/headstart-sdk'
import { NumberCanCancelOrReturn } from 'src/app/services/lineitem-status.helper'

export class LineItemForm {
  id = new FormControl()
  selected = new FormControl()
  quantityToReturnOrCancel = new FormControl()
  returnReason = new FormControl()
  lineItem: HSLineItem

  constructor(lineItem: HSLineItem, action: string) {
    if (lineItem.ID) this.id.setValue(lineItem.ID)
    const amountCanBeReturnedOrCanceled = NumberCanCancelOrReturn(
      lineItem,
      action
    )
    this.lineItem = lineItem
    this.selected.setValue(false)
    if (!amountCanBeReturnedOrCanceled) {
      this.selected.disable()
    }
    this.quantityToReturnOrCancel.disable()
    this.quantityToReturnOrCancel.setValidators([
      Validators.required,
      Validators.min(1),
      Validators.max(amountCanBeReturnedOrCanceled),
    ])
    this.returnReason.disable()
    this.returnReason.setValidators([Validators.required])
  }
}
