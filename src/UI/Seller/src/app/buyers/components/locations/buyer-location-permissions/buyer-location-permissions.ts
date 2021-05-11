import { Component, Input } from '@angular/core'
import { UserGroupAssignment, User, ListPage, ApprovalRule } from '@ordercloud/angular-sdk'
import { BuyerLocationService } from '../buyer-location.service'
import { REDIRECT_TO_FIRST_PARENT } from '@app-seller/layout/header/header.config'
import { PermissionTypes } from '../buyer-location-permissions/buyer-location-permissions.constants'
import { BuyerUserService } from '../../users/buyer-user.service'
import { PermissionType } from '@app-seller/models/user.types'
import { ApprovalRules } from 'ordercloud-javascript-sdk'
import { TranslateService } from '@ngx-translate/core'

@Component({
  selector: 'buyer-location-permissions',
  templateUrl: './buyer-location-permissions.component.html',
  styleUrls: ['./buyer-location-permissions.component.scss'],
})
export class BuyerLocationPermissions {
  locationPermissionsAssigmentsEditable: UserGroupAssignment[] = []
  locationPermissionsAssigmentsStatic: UserGroupAssignment[] = []
  add: UserGroupAssignment[]
  del: UserGroupAssignment[]
  areChanges = false
  locationUsers: ListPage<User>
  _locationID: string
  permissionTypes: PermissionType[] = PermissionTypes
  requestedUserConfirmation = false
  locationApprovals: ListPage<ApprovalRule>
  locationApprovalsTooltip: string

  @Input()
  set locationID(value: string) {
    this._locationID = value
    if (value && value !== REDIRECT_TO_FIRST_PARENT) {
      this.updateUserPermissionAssignments(value)
      this.updateLocationUsers(value)
    }
  }

  constructor(
    private buyerLocationService: BuyerLocationService,
    private buyerUserService: BuyerUserService,
    private translate: TranslateService
  ) {}

  async changePage(page: number): Promise<void> {
    await this.updateLocationUsers(this._locationID, page)
  }
  async updateLocationUsers(locationID: string, page?: number): Promise<void> {
    this.locationUsers = await this.buyerLocationService.getLocationUsers(
      locationID, page
    )
  }

  async updateUserPermissionAssignments(locationID: string): Promise<void> {
    const buyerID = locationID.split("-")[0]
    const [approvals, assignments] = await Promise.all([
      ApprovalRules.List(buyerID, {filters: {"ApprovingGroupID": `${locationID}-OrderApprover`}}),
      this.buyerLocationService.getLocationPermissions(locationID)
    ])
    this.locationApprovalsTooltip = approvals?.Items?.length < 1 ? 
    this.translate.instant('ADMIN.PERMISSIONS.LOCATION_APPROVAL_TOOLTIP') : ''
    this.locationApprovals = approvals
    this.locationPermissionsAssigmentsEditable = assignments
    this.locationPermissionsAssigmentsStatic = JSON.parse(
      JSON.stringify(this.locationPermissionsAssigmentsEditable)
    )
    this.checkForUserUserGroupAssignmentChanges()
  }

  isPermissionDisabled(userGroupSuffix: string, userID: string) {
    return (userGroupSuffix === 'OrderApprover' && 
      this.locationApprovals?.Items?.length < 1 && 
      !this.isAssigned(userID, userGroupSuffix))
  }

  toggleUserUserGroupAssignment(userID: string, userGroupSuffix: string): void {
    const userGroupID = `${this._locationID}-${userGroupSuffix}`
    if (this.isAssigned(userID, userGroupSuffix)) {
      this.locationPermissionsAssigmentsEditable = this.locationPermissionsAssigmentsEditable.filter(
        (groupAssignment) =>
          groupAssignment.UserGroupID !== userGroupID ||
          groupAssignment.UserID !== userID
      )
    } else {
      const newUserUserGroupAssignment = {
        UserID: userID,
        UserGroupID: userGroupID,
      }
      this.locationPermissionsAssigmentsEditable = [
        ...this.locationPermissionsAssigmentsEditable,
        newUserUserGroupAssignment,
      ]
    }
    this.checkForUserUserGroupAssignmentChanges()
  }

  isAssigned(userID: string, permissionType: string): boolean {
    return this.locationPermissionsAssigmentsEditable.some(
      (l) => l.UserID === userID && l.UserGroupID.includes(permissionType)
    )
  }

  discardUserUserGroupAssignmentChanges(): void {
    this.locationPermissionsAssigmentsEditable = this.locationPermissionsAssigmentsStatic
    this.checkForUserUserGroupAssignmentChanges()
  }

  checkForUserUserGroupAssignmentChanges(): void {
    this.add = this.locationPermissionsAssigmentsEditable.filter(
      (editableAssignment) =>
        !this.locationPermissionsAssigmentsStatic.some(
          (staticAssignment) =>
            staticAssignment.UserGroupID === editableAssignment.UserGroupID &&
            staticAssignment.UserID === editableAssignment.UserID
        )
    )
    this.del = this.locationPermissionsAssigmentsStatic.filter(
      (staticAssignment) =>
        !this.locationPermissionsAssigmentsEditable.some(
          (editableAssignment) =>
            editableAssignment.UserGroupID === staticAssignment.UserGroupID &&
            editableAssignment.UserID === staticAssignment.UserID
        )
    )
    this.areChanges = this.add.length > 0 || this.del.length > 0
  }

  requestUserConfirmation(): void {
    this.requestedUserConfirmation = true
  }

  async executeUserUserGroupAssignmentRequests(): Promise<void> {
    this.requestedUserConfirmation = false
    await this.buyerUserService.updateBuyerPermissionGroupAssignments(
      this._locationID,
      this.add,
      this.del
    )
    this.locationPermissionsAssigmentsStatic = this.locationPermissionsAssigmentsEditable
    this.checkForUserUserGroupAssignmentChanges()
  }
}
