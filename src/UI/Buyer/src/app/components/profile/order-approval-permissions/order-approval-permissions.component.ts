import { Component, Input } from '@angular/core'
import { UserGroup, UserGroupAssignment } from 'ordercloud-javascript-sdk'
import { HeadStartSDK, HSUser } from '@ordercloud/headstart-sdk'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'

@Component({
  templateUrl: './order-approval-permissions.component.html',
  styleUrls: ['./order-approval-permissions.component.scss'],
})
export class OCMOrderAccessManagement {
  users: HSUser[] = []
  permissionAssignmentsStatic: UserGroupAssignment[] = []
  permissionAssignmentsEditable: UserGroupAssignment[] = []
  add: UserGroupAssignment[] = []
  del: UserGroupAssignment[] = []
  areChanges = false
  requestedUserConfirmation = false
  currentLocation: UserGroup = null
  currentLocationApprovalThresholdStatic = 0
  currentLocationApprovalThresholdEditable = 0
  hasApprovalStatic: boolean
  hasApprovalEditable: boolean
  areAllUsersAssignedToNeedsApproval = false
  _locationID = ''

  @Input() set locationID(value: string) {
    this._locationID = value
    this.fetchUserManagementInformation()
  }

  constructor(private context: ShopperContextService) { }

  toggleAllNeedingApproval(): void {
    if (this.areAllUsersAssignedToNeedsApproval) {
      this.setNeedApprovalForNoUsers()
    } else {
      this.setNeedApprovalForAllUsers()
    }
  }

  checkIfAllUsersAreAssignedToNeedsApproval(): void {
    this.areAllUsersAssignedToNeedsApproval = this.users.every((u) =>
      this.permissionAssignmentsEditable.some(
        (a) => a.UserID === u.ID && a.UserGroupID.includes('NeedsApproval')
      )
    )
  }

  setNeedApprovalForAllUsers(): void {
    this.setNeedApprovalForNoUsers()
    this.permissionAssignmentsEditable = [
      ...this.permissionAssignmentsEditable,
      ...this.users.map((u) => {
        return {
          UserID: u.ID,
          UserGroupID: `${this._locationID}-NeedsApproval`,
        }
      }),
    ]
    this.checkForChanges()
  }

  setNeedApprovalForNoUsers(): void {
    this.permissionAssignmentsEditable = this.permissionAssignmentsEditable.filter(
      (c) => !c.UserGroupID.includes('NeedsApproval')
    )
    this.checkForChanges()
  }

  async fetchUserManagementInformation(): Promise<void> {
    this.users = (
      await this.context.userManagementService.getLocationUsers(
        this._locationID
      )
    ).Items
    try {
      await this.updateAssignments()
      const currentThreshold = await this.context.userManagementService.getLocationApprovalThreshold(
        this._locationID
      )
      this.setApprovalRuleValues(currentThreshold)
    } catch (ex) {
      if (ex.status === 404) {
        this.setApprovalRuleValues()
      } else {
        throw ex
      }
    }
  }

  toggleApproval() {
    this.hasApprovalEditable = !this.hasApprovalEditable
    if (!this.hasApprovalEditable) {
      this.currentLocationApprovalThresholdEditable = this.currentLocationApprovalThresholdStatic
    }
  }

  resetApprovals() {
    this.hasApprovalEditable = this.hasApprovalStatic
    this.currentLocationApprovalThresholdEditable = this.currentLocationApprovalThresholdStatic
  }

  setApprovalRuleValues(amount?: number): void {
    if (amount >=0) {
      this.hasApprovalStatic = true
      this.hasApprovalEditable = true
      this.currentLocationApprovalThresholdStatic = amount
      this.currentLocationApprovalThresholdEditable = this.currentLocationApprovalThresholdStatic
      this.checkIfAllUsersAreAssignedToNeedsApproval()
    } else {
      this.hasApprovalStatic = false
      this.hasApprovalEditable = false
    }
  }

  approvalChanged() {
    return !(
      this.hasApprovalEditable === this.hasApprovalStatic &&
      this.currentLocationApprovalThresholdEditable === this.currentLocationApprovalThresholdStatic
    )
  }

  async updateAssignments(): Promise<void> {
    this.permissionAssignmentsStatic = await this.context.userManagementService.getLocationApprovalPermissions(
      this._locationID
    )
    this.permissionAssignmentsEditable = [...this.permissionAssignmentsStatic]
    this.checkForChanges()
  }

  setThreshold(value: number): void {
    this.currentLocationApprovalThresholdEditable = value
  }

  setThresholdFromEvent(event: any): void {
    this.currentLocationApprovalThresholdEditable = Number(event.target.value)
  }

  async saveNewThreshold(): Promise<void> {
    const approval = HeadStartSDK.Services.BuildApproval(
      this._locationID,
      this.currentLocationApprovalThresholdEditable)
    const buyerID = this._locationID.split('-')[0]
    if (this.hasApprovalEditable) {
      const newApproval = await HeadStartSDK.ApprovalRules.SaveApprovalRule(buyerID, this._locationID, approval)
      const newThreshold = parseFloat(newApproval.RuleExpression.split(">")[1])
      this.setApprovalRuleValues(newThreshold)
    } else {
      await HeadStartSDK.ApprovalRules.DeleteApprovalRule(buyerID, this._locationID, this._locationID)
      this.setApprovalRuleValues()
    }

  }

  isAssigned(userID: string, assignmentType: string): boolean {
    return this.permissionAssignmentsEditable.some(
      (n) => n.UserID === userID && n.UserGroupID.includes(assignmentType)
    )
  }

  toggleAssignment(userID: string, assignmentType: string): void {
    if (this.isAssigned(userID, assignmentType)) {
      this.permissionAssignmentsEditable = this.permissionAssignmentsEditable.filter(
        (n) => !(n.UserID === userID && n.UserGroupID.includes(assignmentType))
      )
    } else {
      this.permissionAssignmentsEditable = [
        ...this.permissionAssignmentsEditable,
        {
          UserID: userID,
          UserGroupID: `${this._locationID}-${assignmentType}`,
        },
      ]
    }
    this.checkForChanges()
  }

  checkForChanges(): void {
    this.add = this.permissionAssignmentsEditable.filter(
      (editableAssignment) =>
        !this.permissionAssignmentsStatic.some(
          (staticAssignment) =>
            staticAssignment.UserID === editableAssignment.UserID &&
            staticAssignment.UserGroupID === editableAssignment.UserGroupID
        )
    )
    this.del = this.permissionAssignmentsStatic.filter(
      (staticAssignment) =>
        !this.permissionAssignmentsEditable.some(
          (editableAssignment) =>
            staticAssignment.UserID === editableAssignment.UserID &&
            staticAssignment.UserGroupID === editableAssignment.UserGroupID
        )
    )
    this.areChanges = this.add.length > 0 || this.del.length > 0
    this.checkIfAllUsersAreAssignedToNeedsApproval()
  }

  discardUserUserGroupAssignmentChanges(): void {
    this.permissionAssignmentsEditable = this.permissionAssignmentsStatic
    this.checkForChanges()
  }

  requestUserConfirmation(): void {
    this.requestedUserConfirmation = true
  }

  async executeUserUserGroupAssignmentRequests(): Promise<void> {
    const buyerID = this._locationID.split('-')[0]
    await this.context.userManagementService.updateUserUserGroupAssignments(
      buyerID,
      this._locationID,
      this.add,
      this.del
    )
    this.permissionAssignmentsStatic = this.permissionAssignmentsEditable
    this.requestedUserConfirmation = false
    this.checkForChanges()
  }
}
