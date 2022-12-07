import { Component, Input, Output, EventEmitter } from '@angular/core'
import {
  HSOrder,
  HSLineItem,
  OrderDetails,
  HSOrderReturn,
} from '@ordercloud/headstart-sdk'
import { groupBy } from 'lodash'
import { FormGroup, FormArray, FormBuilder } from '@angular/forms'
import { ReturnRequestForm } from './order-return-request-table/models/return-request-form.model'
import { OrderReturnItem, OrderReturns } from 'ordercloud-javascript-sdk'
import { orderBy } from 'lodash'
import { CanReturn } from '@app-seller/orders/line-item-status.helper'

@Component({
  selector: 'order-return-request-form',
  templateUrl: './order-return-request-form.component.html',
  styleUrls: ['./order-return-request-form.component.scss'],
})
export class OrderReturnRequestForm {
  @Input() set orderDetails(value: OrderDetails) {
    this.order = value.Order
    this.lineItems = value.LineItems
    this.orderReturns = value.OrderReturns
    this.setData()
  }
  @Output() returnCreated = new EventEmitter()

  order: HSOrder
  lineItems: HSLineItem[]
  orderReturns: HSOrderReturn[]
  liGroupedByShipFrom: HSLineItem[][]
  quantitiesToReturn: number[] = []
  requestReturnForm: FormGroup
  groupedLineItemsToReturn: FormArray
  isSaving = false

  constructor(private fb: FormBuilder) {}

  setData(): void {
    this.lineItems = this.lineItems.filter((li) =>
      CanReturn(li, this.orderReturns)
    )
    const liGroups = groupBy(this.lineItems, (li) => li.ShipFromAddressID)
    this.liGroupedByShipFrom = Object.values(liGroups)
    this.setRequestReturnForm()
  }

  isAnyRowSelected(): boolean {
    const liGroups = this.requestReturnForm.controls.liGroups as FormArray
    const selectedItem = liGroups.value.find((value) =>
      value.lineItems.find((lineItem) => lineItem.selected === true)
    )
    return Boolean(selectedItem)
  }

  setRequestReturnForm(): void {
    this.requestReturnForm = this.fb.group(
      new ReturnRequestForm(
        this.fb,
        this.order.ID,
        this.liGroupedByShipFrom,
        this.orderReturns
      )
    )
  }

  async submitReturn(): Promise<void> {
    const allLineItems = this.requestReturnForm.value.liGroups.reduce(
      (acc, current) => {
        return [...acc, ...current.lineItems]
      },
      []
    )
    const lineItemsToSubmitReturn = allLineItems.filter((li) => li.selected)
    const lineItemChanges = lineItemsToSubmitReturn.map((info) => {
      return {
        LineItemID: info.lineItem.ID,
        Quantity: info.quantityToReturn,
        Comments: info.returnReason,
      }
    }) as OrderReturnItem[]
    const createdReturn = await OrderReturns.Create({
      OrderID: this.order.ID,
      ID: this.buildReturnId(this.order.ID, this.orderReturns),
      ItemsToReturn: lineItemChanges, // used for logs
      Comments: this.requestReturnForm.controls.comments.value as string,
    })
    await OrderReturns.Patch(createdReturn.ID, {
      xp: {
        InitialRefundAmount: createdReturn.RefundAmount,
      },
    } as HSOrderReturn)
    await OrderReturns.Submit(createdReturn.ID)
  }

  buildReturnId(orderID: string, orderReturns: HSOrderReturn[]): string {
    if (!orderReturns.length) {
      return `${orderID}R`
    }
    const lastReturn = orderBy(orderReturns, 'DateCreated', 'desc')[0]
    return `${lastReturn.ID}R`
  }

  async onSubmit(): Promise<void> {
    this.isSaving = true
    await this.submitReturn()
    this.isSaving = false
    this.returnCreated.emit()
  }
}
