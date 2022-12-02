import { Component, Input, Output, EventEmitter } from '@angular/core'
import {
  HSOrder,
  HSLineItem,
  OrderDetails,
  HSOrderReturn,
} from '@ordercloud/headstart-sdk'
import { groupBy as _groupBy, flatten as _flatten } from 'lodash'
import { FormGroup, FormArray, FormBuilder } from '@angular/forms'
import { ReturnRequestForm } from './order-return-table/models/return-request-form.model'
import { CanReturn } from 'src/app/services/lineitem-status.helper'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { LineItemGroupSupplier } from 'src/app/models/line-item.types'
import { OrderReturnItem, OrderReturns } from 'ordercloud-javascript-sdk'
import { orderBy } from 'lodash'

@Component({
  templateUrl: './order-return-request-form.component.html',
  styleUrls: ['./order-return-request-form.component.scss'],
})
export class OCMOrderReturnRequestForm {
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
  suppliers: LineItemGroupSupplier[]
  liGroupedByShipFrom: HSLineItem[][]
  quantitiesToReturn: number[] = []
  requestReturnForm: FormGroup
  groupedLineItemsToReturn: FormArray
  isSaving = false

  constructor(
    private context: ShopperContextService,
    private fb: FormBuilder
  ) {}

  setData(): void {
    this.lineItems = this.lineItems.filter((li) =>
      CanReturn(li, this.orderReturns)
    )
    const liGroups = _groupBy(this.lineItems, (li) => li.ShipFromAddressID)
    this.liGroupedByShipFrom = Object.values(liGroups)
    this.setSupplierData(this.liGroupedByShipFrom)
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

  async setSupplierData(liGroups: HSLineItem[][]): Promise<void> {
    this.suppliers = await this.context.orderHistory.getLineItemSuppliers(
      liGroups
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
