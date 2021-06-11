import { Component, EventEmitter, Input, Output } from '@angular/core'
import { FormControl, FormGroup } from '@angular/forms'
import { HeadStartSDK, HSBuyerLocation } from '@ordercloud/headstart-sdk'
import { ApprovalRule, ApprovalRules } from 'ordercloud-javascript-sdk'

@Component({
  selector: 'buyer-location-approvals',
  templateUrl: './buyer-location-approvals.component.html',
})
export class BuyerLocationApprovals {
  _approvalRule: ApprovalRule
  _initialThreshold: number
  editedApproval: ApprovalRule
  approvalForm: FormGroup
  approvalEnabled: boolean
  areChanges = false
  dataIsSaving = false
  @Input()
  set approvalRule(value: ApprovalRule) {
    this.areChanges = false
    this._approvalRule = value
    this._initialThreshold = this.getOrderThreshold(value?.RuleExpression)
    this.buildApprovalForm(value)
  }
  @Input() buyerGroup: HSBuyerLocation
  @Output() approvalUpdated = new EventEmitter<ApprovalRule>()

  buildApprovalForm(approval?: ApprovalRule): void {
    this.approvalEnabled = this.locationHasApproval()
    this.approvalForm = new FormGroup({
      Enabled: new FormControl(this.approvalEnabled),
      OrderThreshold: new FormControl(
        this.getOrderThreshold(approval?.RuleExpression)
      ),
    })
  }

  getOrderThreshold(approvalRule?: string): number {
    return approvalRule ? parseFloat(approvalRule?.split('>')[1]) : 0
  }

  handleFormChange(event: any) {
    this.checkForChanges()
  }

  checkForChanges(): void {
    const hadApproval = this.locationHasApproval()
    const form = this.approvalForm.getRawValue()
    if (this.approvalEnabled !== hadApproval) {
      this.areChanges = true
    } else {
      if (this.approvalEnabled && hadApproval) {
        this.areChanges =
          parseFloat(form.OrderThreshold) === this._initialThreshold
            ? false
            : true
      } else {
        this.areChanges = false
      }
    }
  }

  handleToggle(event: any) {
    this.approvalEnabled = event.target.checked
    this.checkForChanges()
  }

  locationHasApproval(): boolean {
    return this._approvalRule && this._approvalRule?.ID ? true : false
  }

  discardApprovalChanges() {
    this.buildApprovalForm(this._approvalRule)
    this.areChanges = false
  }

  async saveChanges() {
    try {
      this.dataIsSaving = true
      const buyerID = this.buyerGroup?.UserGroup?.ID?.split('-')[0]
      if (this.approvalEnabled) {
        const form = this.approvalForm.getRawValue()
        const editedApproval = HeadStartSDK.Services.BuildApproval(
          this._approvalRule?.ID || this.buyerGroup?.UserGroup?.ID,
          form.OrderThreshold
        )
        const update = await ApprovalRules.Save(
          buyerID,
          editedApproval.ID,
          editedApproval
        )
        this.approvalUpdated.emit(update)
      } else {
        await ApprovalRules.Delete(buyerID, this._approvalRule.ID)
        this.approvalUpdated.emit()
      }
    } finally {
      this.dataIsSaving = false
      this.areChanges = false
    }
  }
}
