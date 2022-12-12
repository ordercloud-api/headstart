import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core'
import {
  HSOrder,
  HSLineItem,
  HSOrderReturn,
  HeadStartSDK,
} from '@ordercloud/headstart-sdk'
import { flatten, groupBy } from 'lodash'
import { FormGroup, FormArray, FormBuilder } from '@angular/forms'
import { ReturnRequestForm } from './order-return-request-table/models/return-request-form.model'
import {
  MeUser,
  OrderReturnItem,
  OrderReturns,
} from 'ordercloud-javascript-sdk'
import { orderBy } from 'lodash'
import { CanReturn } from '@app-seller/orders/line-item-status.helper'
import { LineItemForm } from './order-return-request-table/models/line-item-form.model'
import { faQuestionCircle } from '@fortawesome/free-solid-svg-icons'
import { NgbActiveModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'

@Component({
  selector: 'order-return-create-modal',
  templateUrl: './order-return-create-modal.component.html',
  styleUrls: ['./order-return-create-modal.component.scss'],
})
export class OrderReturnCreateModal implements OnInit {
  @Input() order: HSOrder
  @Input() lineItems: HSLineItem[]
  @Input() orderReturns: HSOrderReturn[]
  @Output() returnCreated = new EventEmitter()

  currentUser: MeUser
  liGroupedByShipFrom: HSLineItem[][]
  quantitiesToReturn: number[] = []
  requestReturnForm: FormGroup
  groupedLineItemsToReturn: FormArray
  isLoadingCalculate = false
  isLoadingCreate = false
  faQuestionCircle = faQuestionCircle

  constructor(
    private fb: FormBuilder,
    private activeModal: NgbActiveModal,
    private currentUserService: CurrentUserService
  ) {}

  async ngOnInit(): Promise<void> {
    this.lineItems = this.lineItems.filter((li) =>
      CanReturn(li, this.orderReturns)
    )
    const liGroups = groupBy(this.lineItems, (li) => li.ShipFromAddressID)
    this.liGroupedByShipFrom = Object.values(liGroups)
    this.setRequestReturnForm()
    this.currentUser = await this.currentUserService.getUser()
  }

  isAnyRowSelected(): boolean {
    const selectedInputs = this.getSelectedInputs()
    return Boolean(selectedInputs?.length)
  }

  setRequestReturnForm(): void {
    this.requestReturnForm = this.fb.group(
      new ReturnRequestForm(
        this.fb,
        this.order,
        this.liGroupedByShipFrom,
        this.orderReturns
      )
    )

    this.requestReturnForm.valueChanges.subscribe(() => {
      const selectedInputs = this.getSelectedInputs()
      if (
        selectedInputs.length &&
        this.requestReturnForm.controls.refundAmount.value
      ) {
        this.requestReturnForm.controls.refundAmount.setValue(null)
      }
    })
  }

  async calculateRefunds(): Promise<void> {
    this.isLoadingCalculate = true
    try {
      const items = this.getSelectedOrderReturnItems()
      const lineitemCalcs = await HeadStartSDK.OrderReturns.Calculate(
        this.order.ID,
        items
      )
      const selectedInputs = this.getSelectedInputs()
      lineitemCalcs.forEach(({ LineItemID, RefundAmount }) => {
        const input = selectedInputs.find(
          (input) => input.id.value === LineItemID
        )
        if (input) {
          input.refundAmount.setValue(RefundAmount)
        }
      })
    } finally {
      this.isLoadingCalculate = false
    }
  }

  async createReturn(): Promise<void> {
    this.isLoadingCreate = true
    try {
      const itemsToReturn = this.getSelectedOrderReturnItems()
      const createdReturn = await OrderReturns.Create({
        OrderID: this.order.ID,
        ID: this.buildReturnId(this.order.ID, this.orderReturns),
        ItemsToReturn: itemsToReturn,
        RefundAmount: this.requestReturnForm.controls.refundAmount
          .value as number,
        xp: {
          SubmittedStatusDetails: {
            ProcessedByCompanyId: null,
            ProcessedByName: `${this.currentUser.FirstName} ${this.currentUser.LastName}`,
            ProcessedByUserId: `${this.currentUser.ID}`,
          },
          SellerComments: this.requestReturnForm.controls.comments
            .value as string,
        },
      } as HSOrderReturn)
      await OrderReturns.Patch(createdReturn.ID, {
        xp: {
          SubmittedStatusDetails: {
            RefundAmount: createdReturn.RefundAmount, // this is a calculated value, get from created return to avoid chance for errors
          },
        },
      } as HSOrderReturn)
      await OrderReturns.Submit(createdReturn.ID)

      if (this.requestReturnForm.controls.refundImmediately.value) {
        await HeadStartSDK.OrderReturns.Complete(createdReturn.ID)
        await OrderReturns.Patch(createdReturn.ID, {
          xp: {
            CompletedStatusDetails: {
              RefundAmount: createdReturn.RefundAmount,
              ProcessedByUserId: this.currentUser.ID,
              ProcessedByName: `${this.currentUser.FirstName} ${this.currentUser.LastName}`,
              ProcessedByCompanyId: this.currentUser.Supplier?.ID,
            },
          },
        } as HSOrderReturn)
      }
      this.activeModal.close(createdReturn.ID)
    } finally {
      this.isLoadingCreate = false
    }
  }

  getSelectedInputs(): LineItemForm[] {
    const form = this.requestReturnForm as any
    if (!form) {
      return []
    }
    // eslint-disable-next-line @typescript-eslint/no-unnecessary-type-assertion
    const allInputs = flatten(
      form.controls.liGroups.controls.map((liGroupForm) =>
        liGroupForm.controls.lineItems.controls.map(
          (lineitemForm) => lineitemForm.controls
        )
      )
    ) as LineItemForm[]

    return allInputs.filter((input) => input.selected.value)
  }

  unselectItems(): void {
    const selectedInputs = this.getSelectedInputs()
    selectedInputs.forEach((input) => {
      input.selected.setValue(false)
      input.refundAmount.setValue(null)
    })
  }

  getSelectedOrderReturnItems(): OrderReturnItem[] {
    const selectedInputs = this.getSelectedInputs()
    return selectedInputs.map((info) => {
      return {
        LineItemID: (info.lineItem.value as HSLineItem).ID,
        Quantity: info.quantityToReturn.value as number,
        Comments: info.returnReason.value as string,
        RefundAmount: info.refundAmount.value as number,
      }
    }) as OrderReturnItem[]
  }

  buildReturnId(orderID: string, orderReturns: HSOrderReturn[]): string {
    if (!orderReturns.length) {
      return `${orderID}R`
    }
    const lastReturn = orderBy(orderReturns, 'DateCreated', 'desc')[0]
    return `${lastReturn.ID}R`
  }

  async onCreate(): Promise<void> {
    await this.createReturn()
    this.returnCreated.emit()
  }
}
