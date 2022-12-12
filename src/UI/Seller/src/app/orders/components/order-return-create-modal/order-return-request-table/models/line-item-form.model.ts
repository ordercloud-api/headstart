import { FormControl, Validators } from '@angular/forms'
import { NumberCanReturn } from '@app-seller/orders/line-item-status.helper'
import { HSLineItem } from '@ordercloud/headstart-sdk'
import { HSOrderReturn } from '@ordercloud/headstart-sdk'

export class LineItemForm {
  id = new FormControl()
  selected = new FormControl()
  quantityToReturn = new FormControl()
  refundAmount = new FormControl(null)
  returnReason = new FormControl()
  lineItem = new FormControl()

  constructor(lineItem: HSLineItem, orderReturns: HSOrderReturn[]) {
    if (lineItem.ID) this.id.setValue(lineItem.ID)
    const amountCanBeReturned = NumberCanReturn(lineItem, orderReturns)
    this.lineItem.setValue(lineItem)
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
