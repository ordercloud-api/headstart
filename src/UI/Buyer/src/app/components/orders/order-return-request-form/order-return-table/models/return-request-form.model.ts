import { FormArray, FormControl, FormBuilder, Validators } from '@angular/forms'
import { HSLineItem, HSOrderReturn } from '@ordercloud/headstart-sdk'
import { LineItemGroupForm } from './line-item-group-form.model'

export class ReturnRequestForm {
  orderID = new FormControl()
  comments = new FormControl('', [Validators.maxLength(2000)])
  liGroups = new FormArray([])

  constructor(
    private fb: FormBuilder,
    orderID: string,
    liGroups: HSLineItem[][],
    orderReturns: HSOrderReturn[]
  ) {
    if (orderID) {
      this.orderID.setValue(orderID)
    }
    liGroups.forEach((liGroup) =>
      this.liGroups.push(
        this.fb.group(new LineItemGroupForm(fb, liGroup, orderReturns))
      )
    )
  }
}
