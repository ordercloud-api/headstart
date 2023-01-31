import { Component, Input, Output, EventEmitter, OnInit } from '@angular/core'
import {
  HeadStartSDK,
  HSLineItem,
  HSOrder,
  HSOrderReturn,
} from '@ordercloud/headstart-sdk'
import { NgbModal, NgbModalRef } from '@ng-bootstrap/ng-bootstrap'
import { UntypedFormBuilder } from '@angular/forms'
import { TypedFormGroup } from 'ngx-forms-typed'
import { MeUser, OrderReturns } from 'ordercloud-javascript-sdk'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { RefundInputChange } from '../order-return-refund-input/order-return-refund-input.component'

type ApprovalFormValue = {
  Comments?: string
  RefundImmediately?: boolean
}
type ApprovalForm = TypedFormGroup<ApprovalFormValue>

@Component({
  selector: 'order-return-summary',
  templateUrl: './order-return-summary.component.html',
  styleUrls: ['./order-return-summary.component.scss'],
})
export class OrderReturnSummaryComponent implements OnInit {
  @Input() orderReturn: HSOrderReturn
  @Input() order: HSOrder
  @Input() lineItems: HSLineItem[]
  @Output() updateReturn = new EventEmitter<HSOrderReturn>()
  modalReference: NgbModalRef
  approvalForm: ApprovalForm
  declineForm: ApprovalForm
  dataIsSaving = false
  currentUser: MeUser

  constructor(
    private modalService: NgbModal,
    private formBuilder: UntypedFormBuilder,
    private currentUserService: CurrentUserService
  ) {}

  async ngOnInit(): Promise<void> {
    this.buildForms()
    this.currentUser = await this.currentUserService.getUser()
  }

  private buildForms() {
    this.approvalForm = this.formBuilder.group({
      Comments: [''],
      RefundImmediately: [false],
    }) as ApprovalForm

    this.declineForm = this.formBuilder.group({
      Comments: [''],
    }) as ApprovalForm
  }

  openModal(content: any): void {
    this.modalReference = this.modalService.open(content, {
      ariaLabelledBy: 'confirm-modal',
    })
  }

  async updateLineItemRefundAmount(changes: RefundInputChange): Promise<void> {
    this.dataIsSaving = true
    try {
      const itemsToReturn = this.orderReturn.ItemsToReturn.map((item) => {
        if (item.LineItemID === changes.lineItemId) {
          item.RefundAmount = changes.refundAmount
        }
        return item
      })
      const patchedOrderReturn = await OrderReturns.Patch(this.orderReturn.ID, {
        ItemsToReturn: itemsToReturn,
      })
      this.updateReturn.emit(patchedOrderReturn)
    } finally {
      this.dataIsSaving = false
    }
  }

  async approveReturn(): Promise<void> {
    this.dataIsSaving = true
    try {
      await this.ordercloudApproveReturn()
      if (this.approvalForm.controls.RefundImmediately.value) {
        await this.ordercloudCompleteReturn()
      }
      this.approvalForm.controls.Comments.setValue('')
      this.modalReference.close()
    } finally {
      this.dataIsSaving = false
    }
  }

  async declineReturn(): Promise<void> {
    this.dataIsSaving = true
    try {
      await this.ordercloudDeclineReturn()
      this.declineForm.controls.Comments.setValue('')
      this.modalReference.close()
    } finally {
      this.dataIsSaving = false
    }
  }

  async completeReturn(): Promise<void> {
    this.dataIsSaving = true
    try {
      await this.ordercloudCompleteReturn()
      this.modalReference.close()
    } finally {
      this.dataIsSaving = false
    }
  }

  private async ordercloudApproveReturn() {
    const approvedReturn = await OrderReturns.Approve(this.orderReturn.ID, {
      Comments: this.approvalForm.controls.Comments.value,
    })
    const patchedReturn = await OrderReturns.Patch(this.orderReturn.ID, {
      xp: {
        SellerComments: this.approvalForm.controls.Comments.value,
        ApprovedStatusDetails: {
          RefundAmount: approvedReturn.RefundAmount,
          ProcessedByUserId: this.currentUser.ID,
          ProcessedByName: `${this.currentUser.FirstName} ${this.currentUser.LastName}`,
          ProcessedByCompanyId: this.currentUser.Supplier?.ID,
        },
      },
    })
    this.updateReturn.emit(patchedReturn)
  }

  private async ordercloudCompleteReturn() {
    const completedReturn = await HeadStartSDK.OrderReturns.Complete(
      this.orderReturn.ID
    )
    const patchedReturn = await OrderReturns.Patch(this.orderReturn.ID, {
      xp: {
        CompletedStatusDetails: {
          RefundAmount: completedReturn.RefundAmount,
          ProcessedByUserId: this.currentUser.ID,
          ProcessedByName: `${this.currentUser.FirstName} ${this.currentUser.LastName}`,
          ProcessedByCompanyId: this.currentUser.Supplier?.ID,
        },
      },
    } as HSOrderReturn)
    this.updateReturn.emit(patchedReturn)
  }

  private async ordercloudDeclineReturn() {
    const declinedReturn = await OrderReturns.Decline(this.orderReturn.ID, {
      Comments: this.declineForm.controls.Comments.value,
    })
    const patchedReturn = await OrderReturns.Patch(this.orderReturn.ID, {
      xp: {
        SellerComments: this.declineForm.controls.Comments.value,
        DeclinedStatusDetails: {
          RefundAmount: declinedReturn.RefundAmount,
          ProcessedByUserId: this.currentUser.ID,
          ProcessedByName: `${this.currentUser.FirstName} ${this.currentUser.LastName}`,
          ProcessedByCompanyId: this.currentUser.Supplier?.ID,
        },
      },
    } as HSOrderReturn)
    this.updateReturn.emit(patchedReturn)
  }
}
