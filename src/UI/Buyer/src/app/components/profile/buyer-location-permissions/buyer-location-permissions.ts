import { Component, Input } from '@angular/core'
import { UserGroupAssignment } from 'ordercloud-javascript-sdk'
import { HSUser } from '@ordercloud/headstart-sdk'
import { ShopperContextService } from 'src/app/services/shopper-context/shopper-context.service'
import { PermissionType, PermissionTypes } from 'src/app/models/permissions.types'

@Component({
  selector: 'buyer-location-permissions',
  templateUrl: './buyer-location-permissions.component.html',
  styleUrls: ['./buyer-location-permissions.component.scss'],
})
export class OCMBuyerLocationPermissions {
  locationPermissionsAssigmentsEditable: UserGroupAssignment[] = []
  locationPermissionsAssigmentsStatic: UserGroupAssignment[] = []
  add: UserGroupAssignment[]
  del: UserGroupAssignment[]
  areChanges = false
  locationUsers: HSUser[]
  _locationID: string
  permissionTypes: PermissionType[] = PermissionTypes.filter(
    (p) =>
      p.UserGroupSuffix !== 'NeedsApproval' &&
      p.UserGroupSuffix !== 'OrderApprover'
  )
  requestedUserConfirmation = false

  @Input()
  set locationID(value: string) {
    this._locationID = value
    if (value) {
      this.updateUserPermissionAssignments(value)
      this.updateLocationUsers(value)
    }
  }

  constructor(private context: ShopperContextService) {}

  async updateLocationUsers(locationID: string): Promise<void> {
    const locationUsersListPage = await this.context.userManagementService.getLocationUsers(
      locationID
    )
    this.locationUsers = locationUsersListPage.Items
  }

  async updateUserPermissionAssignments(locationID: string): Promise<void> {
    this.locationPermissionsAssigmentsEditable = await this.context.userManagementService.getLocationPermissions(
      locationID
    )
    this.locationPermissionsAssigmentsStatic = JSON.parse(
      JSON.stringify(this.locationPermissionsAssigmentsEditable)
    )
    this.checkForUserUserGroupAssignmentChanges()
  }

  toggleUserUserGroupAssignment(
    userID: string,
    userGroupSuffix: string,
    event: any
  ): void {
    const userGroupID = `${this._locationID}-${userGroupSuffix}`
    if (this.isAssigned(userID, userGroupSuffix)) {
      if (this.isLastPermissionAdmin(userID, userGroupSuffix)) {
        // don't allow buyer user to remove the final permission admin otherwise the location
        // permanently loses the ability to add or remove permissions without seller intervention
        // event prevent default prevents the toggle switch from changing even tho we are
        // already preventing the data from changing
        event.preventDefault()
        throw Error('Cannot remove the final permission admin')
      } else {
        this.locationPermissionsAssigmentsEditable = this.locationPermissionsAssigmentsEditable.filter(
          (groupAssignment) =>
            groupAssignment.UserGroupID !== userGroupID ||
            groupAssignment.UserID !== userID
        )
      }
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

  isLastPermissionAdmin(userID: string, userGroupSuffix: string): boolean {
    return (
      userGroupSuffix === 'PermissionAdmin' &&
      this.locationPermissionsAssigmentsEditable.filter((l) =>
        l.UserGroupID.includes('PermissionAdmin')
      ).length === 1
    )
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
    await this.context.userManagementService.updateUserUserGroupAssignments(
      this._locationID.split('-')[0],
      this._locationID,
      this.add,
      this.del
    )
    this.locationPermissionsAssigmentsStatic = this.locationPermissionsAssigmentsEditable
    this.checkForUserUserGroupAssignmentChanges()
  }
}
