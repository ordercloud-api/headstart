import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import {
  User,
  UserGroupAssignment,
  OcSupplierUserGroupService,
  ListPage,
  UserGroup,
} from '@ordercloud/angular-sdk'
import { SUPPLIER_SUB_RESOURCE_LIST } from '../suppliers/supplier.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { SupplierUsers } from 'ordercloud-javascript-sdk'
import { ListArgs } from '@ordercloud/headstart-sdk'

// TODO - this service is only relevent if you're already on the supplier details page. How can we enforce/inidcate that?
@Injectable({
  providedIn: 'root',
})
export class SupplierUserService extends ResourceCrudService<User> {
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
    private ocSupplierUserGroupService: OcSupplierUserGroupService,
    public currentUserService: CurrentUserService
  ) {
    super(
      router,
      activatedRoute,
      SupplierUsers,
      currentUserService,
      '/suppliers',
      'suppliers',
      SUPPLIER_SUB_RESOURCE_LIST,
      'users',
      '/my-supplier'
    )
  }
  async updateUserUserGroupAssignments(
    supplierID: string,
    add: UserGroupAssignment[],
    del: UserGroupAssignment[]
  ): Promise<void> {
    const addRequests = add.map((newAssignment) =>
      this.addSupplierUserUserGroupAssignment(supplierID, newAssignment)
    )
    const deleteRequests = del.map((assignmentToRemove) =>
      this.removeSupplierUserUserGroupAssignment(supplierID, assignmentToRemove)
    )
    await Promise.all([...addRequests, ...deleteRequests])
  }

  private addSupplierUserUserGroupAssignment(
    supplierID: string,
    assignment: UserGroupAssignment
  ): Promise<void> {
    return this.ocSupplierUserGroupService
      .SaveUserAssignment(supplierID, {
        UserID: assignment.UserID,
        UserGroupID: assignment.UserGroupID,
      })
      .toPromise()
  }

  private removeSupplierUserUserGroupAssignment(
    supplierID: string,
    assignment: UserGroupAssignment
  ): Promise<void> {
    return this.ocSupplierUserGroupService
      .DeleteUserAssignment(
        supplierID,
        assignment.UserGroupID,
        assignment.UserID
      )
      .toPromise()
  }

  async getUserGroups(
    supplierID: string,
    options: ListArgs
  ): Promise<ListPage<UserGroup>> {
    return await this.ocSupplierUserGroupService
      .List(supplierID, options as any)
      .toPromise()
  }

  async listUserAssignments(
    userID: string,
    supplierID: string
  ): Promise<ListPage<UserGroupAssignment>> {
    return await this.ocSupplierUserGroupService
      .ListUserAssignments(supplierID, { userID })
      .toPromise()
  }
}
