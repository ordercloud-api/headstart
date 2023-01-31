import { UntypedFormArray, UntypedFormControl, UntypedFormBuilder } from '@angular/forms'
import { HSLineItem, HSOrderReturn } from '@ordercloud/headstart-sdk'
import { LineItemForm } from './line-item-form.model'

export class LineItemGroupForm {
  shipFromAddressID = new UntypedFormControl()
  lineItems = new UntypedFormArray([])
  liGroup: HSLineItem[]

  constructor(
    private fb: UntypedFormBuilder,
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
