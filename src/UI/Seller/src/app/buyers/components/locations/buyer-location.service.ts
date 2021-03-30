import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import {
  BuyerAddress,
  OcUserGroupService,
  UserGroupAssignment,
  User,
  OcUserService,
} from '@ordercloud/angular-sdk'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { BUYER_SUB_RESOURCE_LIST } from '../buyers/buyer.service'
import { HeadStartSDK } from '@ordercloud/headstart-sdk'
import { BuyerUserService } from '../users/buyer-user.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { Addresses, ListPage } from 'ordercloud-javascript-sdk'
import { PermissionTypes } from './buyer-location-permissions/buyer-location-permissions.constants'



// TODO - this service is only relevent if you're already on the supplier details page. How can we enforce/inidcate that?
@Injectable({
  providedIn: 'root',
})
export class BuyerLocationService extends ResourceCrudService<BuyerAddress> {
  emptyResource = {
    UserGroup: {
      xp: {
        Type: '',
        Currency: null,
      },
      ID: '',
      Name: '',
      Description: '',
    },
    Address: {
      xp: {
        Email: '',
      },
      ID: '',
      DateCreated: '',
      CompanyName: '',
      FirstName: '',
      LastName: '',
      Street1: '',
      Street2: '',
      City: '',
      State: '',
      Zip: '',
      Country: '',
      Phone: '',
      AddressName: '',
    },
  }

  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    private ocUserGroupService: OcUserGroupService,
    private ocUserService: OcUserService,
    private buyerUserService: BuyerUserService,
    public currentUserService: CurrentUserService
  ) {
    super(
      router,
      activatedRoute,
      Addresses,
      currentUserService,
      '/buyers',
      'buyers',
      BUYER_SUB_RESOURCE_LIST,
      'locations'
    )
  }

  async updateResource(originalID: string, resource: any): Promise<any> {
    const resourceID = await this.getParentResourceID()
    const newResource = await HeadStartSDK.ValidatedAddresses.SaveBuyerAddress(
      resourceID,
      originalID,
      resource
    )
    const resourceIndex = this.resourceSubject.value.Items.findIndex(
      (i: any) => i.ID === newResource.ID
    )
    this.resourceSubject.value.Items[resourceIndex] = newResource
    this.resourceSubject.next(this.resourceSubject.value)
    return newResource
  }

  async createNewResource(resource: any): Promise<any> {
    const resourceID = await this.getParentResourceID()
    const newResource = await HeadStartSDK.ValidatedAddresses.CreateBuyerAddress(
      resourceID,
      resource
    )
    this.resourceSubject.value.Items = [
      ...this.resourceSubject.value.Items,
      newResource,
    ]
    this.resourceSubject.next(this.resourceSubject.value)
    return newResource
  }

  async getLocationPermissions(
    locationID: string
  ): Promise<UserGroupAssignment[]> {
    const buyerID = locationID.split('-')[0]
    const requests = PermissionTypes.map((p) =>
      // todo accomodate over 100 users
      this.ocUserGroupService
        .ListUserAssignments(buyerID, {
          userGroupID: `${locationID}-${p.UserGroupSuffix}`,
          pageSize: 100,
        })
        .toPromise()
    )
    const responses = await Promise.all(requests)
    return responses.reduce((acc, value) => acc.concat(value.Items), [])
  }

  async getLocationUsers(locationID: string, page?: number): Promise<ListPage<User>> {
    const buyerID = locationID.split('-')[0]
    const userResponse = await this.ocUserService
      .List(buyerID, { userGroupID: locationID, page: (page || 1) })
      .toPromise()
    return userResponse
  }
}
