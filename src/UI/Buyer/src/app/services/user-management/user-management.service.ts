import { Injectable } from '@angular/core'
import {
  UserGroup,
  UserGroupAssignment,
  Me,
  Tokens,
} from 'ordercloud-javascript-sdk'
import { CurrentUserService } from '../current-user/current-user.service'
import { HttpClient, HttpHeaders } from '@angular/common/http'
import {
  HSUser,
  ListPage,
  HeadStartSDK,
} from '@ordercloud/headstart-sdk'
import { AppConfig } from 'src/app/models/environment.types'

@Injectable({
  providedIn: 'root',
})
export class UserManagementService {
  constructor(
    public currentUserService: CurrentUserService,
    // remove below when sdk is regenerated
    private httpClient: HttpClient,
    private appConfig: AppConfig
  ) {}

  async getLocations(): Promise<UserGroup[]> {
    // todo accomodate more than 100 locations
    const loctions = await Me.ListUserGroups({
      pageSize: 100,
      filters: { 'xp.Type': 'BuyerLocation' },
    })
    return loctions.Items
  }

  async getLocationUsers(
    locationID: string
  ): Promise<ListPage<HSUser>> {
    const buyerID = this.currentUserService.get().Buyer.ID
    return await HeadStartSDK.BuyerLocations.ListLocationUsers(
      buyerID,
      locationID
    )
  }

  async getLocationPermissions(
    locationID: string
  ): Promise<UserGroupAssignment[]> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${Tokens.GetAccessToken()}`,
    })
    const buyerID = locationID.split('-')[0]
    const url = `${this.appConfig.middlewareUrl}/buyerlocations/${buyerID}/${locationID}/permissions`
    return this.httpClient
      .get<UserGroupAssignment[]>(url, { headers })
      .toPromise()
  }

  async getLocationApprovalPermissions(
    locationID: string
  ): Promise<UserGroupAssignment[]> {
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${Tokens.GetAccessToken()}`,
    })
    const buyerID = locationID.split('-')[0]
    const url = `${this.appConfig.middlewareUrl}/buyerlocations/${buyerID}/${locationID}/approvalpermissions`
    return this.httpClient
      .get<UserGroupAssignment[]>(url, { headers })
      .toPromise()
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
    const headers = new HttpHeaders({
      'Content-Type': 'application/json',
      Authorization: `Bearer ${Tokens.GetAccessToken()}`,
    })
    const url = `${this.appConfig.middlewareUrl}/buyerlocations/${buyerID}/${locationID}/approvalthreshold`
    return this.httpClient
      .get<number>(url, { headers })
      .toPromise()
  }
}
