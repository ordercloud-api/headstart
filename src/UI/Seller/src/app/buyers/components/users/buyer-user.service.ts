import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import {
  User,
  UserGroupAssignment,
  UserGroups,
  ListPage,
  UserGroup,
  Users,
} from 'ordercloud-javascript-sdk'
import { BUYER_SUB_RESOURCE_LIST } from '../buyers/buyer.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { CatalogsTempService } from '@app-seller/shared/services/middleware-api/catalogs-temp.service'
import { IUserPermissionsService } from '@app-seller/models/user.types'
import { ListArgs } from '@ordercloud/headstart-sdk'
import { BuyerTempService } from '@app-seller/shared/services/middleware-api/buyer-temp.service'

// TODO - this service is only relevent if you're already on the buyer details page. How can we enforce/inidcate that?
@Injectable({
  providedIn: 'root',
})
export class BuyerUserService
  extends ResourceCrudService<User>
  implements IUserPermissionsService
{
  emptyResource = {
    Username: '',
    FirstName: '',
    LastName: '',
    Email: '',
    Phone: '',
  }

  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    public currentUserService: CurrentUserService,
    private catalogsTempService: CatalogsTempService,
    private buyerTempService: BuyerTempService
  ) {
    super(
      router,
      activatedRoute,
      Users,
      currentUserService,
      '/buyers',
      'buyers',
      BUYER_SUB_RESOURCE_LIST,
      'users'
    )
  }

  async updateBuyerPermissionGroupAssignments(
    buyerLocationID: string,
    add: UserGroupAssignment[],
    del: UserGroupAssignment[]
  ): Promise<void> {
    const buyerID = buyerLocationID.split('-')[0]
    const addRequests = add.map((newAssignment) =>
      this.addBuyerPermissionGroupAssignment(
        buyerID,
        buyerLocationID,
        newAssignment
      )
    )
    const deleteRequests = del.map((assignmentToRemove) =>
      this.removeBuyerUserUserGroupAssignment(buyerID, assignmentToRemove)
    )
    await Promise.all([...addRequests, ...deleteRequests])
  }

  async addBuyerPermissionGroupAssignment(
    buyerID: string,
    buyerLocationID: string,
    assignment: UserGroupAssignment
  ): Promise<void> {
    try {
      await UserGroups.SaveUserAssignment(buyerID, assignment)
    } catch (err) {
      await this.buyerTempService.createPermissionGroup(
        buyerID,
        buyerLocationID,
        assignment.UserGroupID
      )
      await UserGroups.SaveUserAssignment(buyerID, assignment)
    }
  }

  async updateUserUserGroupAssignments(
    buyerID: string,
    add: UserGroupAssignment[],
    del: UserGroupAssignment[],
    shouldSyncUserCatalogAssignments = false
  ): Promise<void> {
    const addRequests = add.map((newAssignment) =>
      this.addBuyerUserUserGroupAssignment(
        buyerID,
        newAssignment,
        shouldSyncUserCatalogAssignments
      )
    )
    const deleteRequests = del.map((assignmentToRemove) =>
      this.removeBuyerUserUserGroupAssignment(
        buyerID,
        assignmentToRemove,
        shouldSyncUserCatalogAssignments
      )
    )
    await Promise.all([...addRequests, ...deleteRequests])
  }

  async addBuyerUserUserGroupAssignment(
    buyerID: string,
    assignment: UserGroupAssignment,
    shouldSyncUserCatalogAssignments = false
  ): Promise<void> {
    await UserGroups.SaveUserAssignment(buyerID, {
      UserID: assignment.UserID,
      UserGroupID: assignment.UserGroupID,
    })
    if (shouldSyncUserCatalogAssignments) {
      await this.catalogsTempService.syncUserCatalogAssignments(
        buyerID,
        assignment.UserID
      )
    }
  }

  async removeBuyerUserUserGroupAssignment(
    buyerID: string,
    assignment: UserGroupAssignment,
    shouldSyncUserCatalogAssignments = false
  ): Promise<void> {
    await UserGroups.DeleteUserAssignment(
      buyerID,
      assignment.UserGroupID,
      assignment.UserID
    )

    if (shouldSyncUserCatalogAssignments) {
      await this.catalogsTempService.syncUserCatalogAssignments(
        buyerID,
        assignment.UserID
      )
    }
  }

  async getUserGroups(
    buyerID: string,
    options: ListArgs
  ): Promise<ListPage<UserGroup>> {
    // temporarily as any until changed to js sdk
    return await UserGroups.List(buyerID, options as any)
  }

  async listUserAssignments(
    userID: string,
    buyerID: string
  ): Promise<ListPage<UserGroupAssignment>> {
    return await UserGroups.ListUserAssignments(buyerID, { userID })
  }

  async createNewResource(resource: User): Promise<any> {
    const buyerID = await this.getParentResourceID()
    resource.ID = buyerID + '-{' + buyerID + '-UserIncrementor' + '}'
    const args = await this.createListArgs([resource])
    const newResource = await this.ocService.Create(...args)
    this.resourceSubject.value.Items = [
      ...this.resourceSubject.value.Items,
      newResource,
    ]
    this.resourceSubject.next(this.resourceSubject.value)
    return newResource
  }
}
