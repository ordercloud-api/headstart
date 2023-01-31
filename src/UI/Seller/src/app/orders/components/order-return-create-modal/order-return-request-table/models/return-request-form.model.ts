/* eslint-disable @typescript-eslint/no-unsafe-return */
/* eslint-disable @typescript-eslint/no-unsafe-call */
/* eslint-disable @typescript-eslint/no-unsafe-member-access */
import {
  UntypedFormArray,
  UntypedFormControl,
  UntypedFormBuilder,
  Validators,
  UntypedFormGroup,
  ValidationErrors,
} from '@angular/forms'
import { AmountCanRefund } from '@app-seller/orders/line-item-status.helper'
import { HSLineItem, HSOrder, HSOrderReturn } from '@ordercloud/headstart-sdk'
import { flatten, sum } from 'lodash'
import { LineItemGroupForm } from './line-item-group-form.model'

export class ReturnRequestForm {
  orderID = new UntypedFormControl()
  comments = new UntypedFormControl('', [Validators.maxLength(2000)])
  refundAmount = new UntypedFormControl(null)
  refundImmediately = new UntypedFormControl(false)
  liGroups = new UntypedFormArray([])

  constructor(
    private fb: UntypedFormBuilder,
    order: HSOrder,
    liGroups: HSLineItem[][],
    orderReturns: HSOrderReturn[]
  ) {
    if (order) {
      this.orderID.setValue(order.ID)
    }
    liGroups.forEach((liGroup) =>
      this.liGroups.push(
        this.fb.group(new LineItemGroupForm(fb, liGroup, orderReturns))
      )
    )
    const amountCanRefund = AmountCanRefund(order, orderReturns)
    this.liGroups.setValidators([maxRefundAmountValidator(amountCanRefund)])
    this.refundAmount.setValidators([Validators.max(amountCanRefund)])
  }
}

function maxRefundAmountValidator(
  maxRefundAmount: number
): (form: UntypedFormGroup) => ValidationErrors | null {
  return (form: any) => {
    // aggregate all of the refund amounts for each line item
    const requestedRefundAmount = sum(
      flatten(
        form.controls.map((liGroupForm) =>
          liGroupForm.controls.lineItems.controls.map(
            (liForm) => liForm.controls.refundAmount.value || 0
          )
        )
      )
    )
    // if requested refund amount is greater than the max refund amount then return form error
    if (requestedRefundAmount > maxRefundAmount) {
      return {
        maxRefund: {
          requested: requestedRefundAmount,
          max: maxRefundAmount,
        },
      }
    }
    return null
  }
}
