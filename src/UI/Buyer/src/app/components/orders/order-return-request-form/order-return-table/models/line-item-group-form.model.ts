import { FormArray, FormControl, FormBuilder } from '@angular/forms'
import { HSLineItem, HSOrderReturn } from '@ordercloud/headstart-sdk'
import { LineItemForm } from './line-item-form.model'

export class LineItemGroupForm {
  shipFromAddressID = new FormControl()
  lineItems = new FormArray([])
  liGroup: HSLineItem[]

  constructor(
    private fb: FormBuilder,
    liGroup: HSLineItem[],
    orderReturns: HSOrderReturn[]
  ) {
    if (liGroup && liGroup[0].ShipFromAddressID) {
      this.shipFromAddressID.setValue(liGroup[0].ShipFromAddressID)
    }
    liGroup.forEach((li) =>
      this.lineItems.push(this.fb.group(new LineItemForm(li, orderReturns)))
    )
    this.liGroup = liGroup
  }
}
