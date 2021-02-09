import { FormArray, FormControl, FormBuilder } from '@angular/forms'
import { HSLineItem } from '@ordercloud/headstart-sdk'
import { LineItemGroupForm } from './line-item-group-form.model'

export class ReturnRequestForm {
  orderID = new FormControl()
  liGroups = new FormArray([])

  constructor(
    private fb: FormBuilder,
    orderID: string,
    liGroups: HSLineItem[][],
    action: string
  ) {
    if (orderID) {
      this.orderID.setValue(orderID)
    }
    liGroups.forEach((liGroup) =>
      this.liGroups.push(
        this.fb.group(new LineItemGroupForm(fb, liGroup, action))
      )
    )
  }
}
