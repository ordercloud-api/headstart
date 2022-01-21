import { Component, Input, Output, EventEmitter } from '@angular/core'
import { HSOrder, HSLineItem, OrderDetails } from '@ordercloud/headstart-sdk'
import { groupBy as _groupBy, flatten as _flatten } from 'lodash'
import { FormGroup, FormArray, FormBuilder } from '@angular/forms'
import { ReturnRequestForm } from './order-return-table/models/return-request-form.model'
import { CanReturnOrCancel } from 'src/app/services/lineitem-status.helper'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { LineItemGroupSupplier } from 'src/app/models/line-item.types'

@Component({
  templateUrl: './order-return.component.html',
  styleUrls: ['./order-return.component.scss'],
})
export class OCMOrderReturn {
  order: HSOrder
  lineItems: HSLineItem[]
  suppliers: LineItemGroupSupplier[]
  liGroupedByShipFrom: HSLineItem[][]
  quantitiesToReturn: number[] = []
  requestReturnForm: FormGroup
  groupedLineItemsToReturn: FormArray
  isSaving = false
  _action: string
  @Input() set orderDetails(value: OrderDetails) {
    this.order = value.Order
    this.lineItems = value.LineItems
    if (this._action) {
      this.setData()
    }
    //  this.setRequestReturnForm();
  }
  @Input() set action(value: string) {
    this._action = value
    if (this.lineItems?.length) {
      this.setData()
    }
  }
  @Output()
  viewReturnFormEvent = new EventEmitter<boolean>()

  constructor(
    private context: ShopperContextService,
    private fb: FormBuilder
  ) {}

  setData(): void {
    this.lineItems = this.lineItems.filter((li) =>
      CanReturnOrCancel(li, this._action)
    )
    //  Need to group lineitems by shipping address and by whether it has been shipped for return/cancel distinction.
    const liGroups = _groupBy(this.lineItems, (li) => li.ShipFromAddressID)
    this.liGroupedByShipFrom = Object.values(liGroups)
    this.setSupplierInfo(this.liGroupedByShipFrom)
    this.setRequestReturnForm(this._action)
  }

  isAnyRowSelected(): boolean {
    const liGroups = this.requestReturnForm.controls.liGroups as FormArray
    const selectedItem = liGroups.value.find((value) =>
      value.lineItems.find((lineItem) => lineItem.selected === true)
    )
    return !!selectedItem
  }

  setRequestReturnForm(action: string): void {
    this.requestReturnForm = this.fb.group(
      new ReturnRequestForm(
        this.fb,
        this.order.ID,
        this.liGroupedByShipFrom,
        action
      )
    )
  }

  async setSupplierInfo(liGroups: HSLineItem[][]): Promise<void> {
    this.suppliers = await this.context.orderHistory.getLineItemSuppliers(
      liGroups
    )
  }

  async submitClaim(): Promise<void> {
    const allLineItems = this.requestReturnForm.value.liGroups.reduce(
      (acc, current) => {
        return [...acc, ...current.lineItems]
      },
      []
    )
    const lineItemsToSubmitClaim = allLineItems.filter((li) => li.selected)
    const lineItemChanges = lineItemsToSubmitClaim.map((claim) => {
      return {
        ID: claim.lineItem.ID,
        Reason: claim.returnReason,
        Quantity: claim.quantityToReturnOrCancel,
      }
    })
    const changeRequest = {
      Status: this._action === 'return' ? 'ReturnRequested' : 'CancelRequested',
      Changes: lineItemChanges,
    }
    await this.context.orderHistory.submitCancelOrReturn(
      this.order.ID,
      changeRequest
    )
  }

  async onSubmit(): Promise<void> {
    this.isSaving = true
    await this.submitClaim()
    this.isSaving = false
    this.viewReturnFormEvent.emit(false)
  }
}
