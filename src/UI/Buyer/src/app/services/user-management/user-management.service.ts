import { Injectable } from '@angular/core'
import { UserGroup, UserGroupAssignment, Me } from 'ordercloud-javascript-sdk'
import { CurrentUserService } from '../current-user/current-user.service'
import { HSUser, ListPage, HeadStartSDK } from '@ordercloud/headstart-sdk'

@Injectable({
  providedIn: 'root',
})
export class UserManagementService {
  constructor(public currentUserService: CurrentUserService) {}

  async getLocations(): Promise<UserGroup[]> {
    // todo accomodate more than 100 locations
    const loctions = await Me.ListUserGroups({
      pageSize: 100,
      filters: { 'xp.Type': 'BuyerLocation' },
    })
    return loctions.Items
  }

  async getLocationUsers(locationID: string): Promise<ListPage<HSUser>> {
    const buyerID = this.currentUserService.get().Buyer.ID
    return await HeadStartSDK.BuyerLocations.ListLocationUsers(
      buyerID,
      locationID
    )
  }

  async getLocationPermissions(
    locationID: string
  ): Promise<UserGroupAssignment[]> {
    const buyerID = locationID.split('-')[0]
    return await HeadStartSDK.BuyerLocations.ListLocationPermissionUserGroups(
      buyerID,
      locationID
    )
  }

  async getLocationApprovalPermissions(
    locationID: string
  ): Promise<UserGroupAssignment[]> {
    const buyerID = locationID.split('-')[0]
    return HeadStartSDK.BuyerLocations.ListLocationApprovalPermissionAsssignments(
      buyerID,
      locationID
    )
  }

  async updateUserUserGroupAssignments(
    buyerID: string,
    locationID: string,
    add: UserGroupAssignment[],
    del: UserGroupAssignment[]
  ): Promise<void> {
    const body = {
      AssignmentsToAdd: add,
      AssignmentsToDelete: del,
    }
    return await HeadStartSDK.BuyerLocations.UpdateLocationPermissions(
      buyerID,
      locationID,
      body
    )
  }

  async getLocationApprovalThreshold(locationID: string): Promise<number> {
    const buyerID = this.currentUserService.get().Buyer.ID
    return HeadStartSDK.BuyerLocations.GetApprovalThreshold(buyerID, locationID)
  }
}
