import { UntypedFormControl, Validators } from '@angular/forms'
import { HSLineItem } from '@ordercloud/headstart-sdk'
import { HSOrderReturn } from '@ordercloud/headstart-sdk'
import { NumberCanReturn } from 'src/app/services/lineitem-status.helper'

export class LineItemForm {
  id = new UntypedFormControl()
  selected = new UntypedFormControl()
  quantityToReturn = new UntypedFormControl()
  returnReason = new UntypedFormControl()
  lineItem: HSLineItem

  constructor(lineItem: HSLineItem, orderReturns: HSOrderReturn[]) {
    if (lineItem.ID) this.id.setValue(lineItem.ID)
    const amountCanBeReturned = NumberCanReturn(lineItem, orderReturns)
    this.lineItem = lineItem
    this.selected.setValue(false)
    if (!amountCanBeReturned) {
      this.selected.disable()
    }
    this.quantityToReturn.setValue(amountCanBeReturned)
    this.quantityToReturn.disable()
    this.quantityToReturn.setValidators([
      Validators.required,
      Validators.min(1),
      Validators.max(amountCanBeReturned),
    ])
    this.returnReason.disable()
    this.returnReason.setValidators([Validators.required])
  }
}
