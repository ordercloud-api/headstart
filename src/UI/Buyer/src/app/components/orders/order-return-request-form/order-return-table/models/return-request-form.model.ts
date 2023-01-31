import { UntypedFormArray, UntypedFormControl, UntypedFormBuilder, Validators } from '@angular/forms'
import { HSLineItem, HSOrderReturn } from '@ordercloud/headstart-sdk'
import { LineItemGroupForm } from './line-item-group-form.model'

export class ReturnRequestForm {
  orderID = new UntypedFormControl()
  comments = new UntypedFormControl('', [Validators.maxLength(2000)])
  liGroups = new UntypedFormArray([])

  constructor(
    private fb: UntypedFormBuilder,
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
