import { Injectable } from '@angular/core'
import { Router, ActivatedRoute } from '@angular/router'
import {
  AdminUserGroups,
  ListPage,
  User,
  UserGroup,
  UserGroupAssignment,
} from 'ordercloud-javascript-sdk'
import { ResourceCrudService } from '@app-seller/shared/services/resource-crud/resource-crud.service'
import { CurrentUserService } from '@app-seller/shared/services/current-user/current-user.service'
import { AdminUsers } from 'ordercloud-javascript-sdk'
import { ListArgs } from '@ordercloud/headstart-sdk'

// TODO - this service is only relevent if you're already on the supplier details page. How can we enforce/inidcate that?
@Injectable({
  providedIn: 'root',
})
export class SellerUserService extends ResourceCrudService<User> {
  constructor(
    router: Router,
    activatedRoute: ActivatedRoute,
    currentUserService: CurrentUserService
  ) {
    super(
      router,
      activatedRoute,
      AdminUsers,
      currentUserService,
      '/seller-admin/users',
      'users'
    )
  }

  emptyResource = {
    Username: '',
    FirstName: '',
    LastName: '',
    Email: '',
    Phone: '',
  }

  async updateUserUserGroupAssignments(
    unusedOrgId: string, // we're adhering to an interface initially made for buyers/suppliers which additionally need an ID (admins don't)
    add: UserGroupAssignment[],
    del: UserGroupAssignment[]
  ): Promise<void> {
    const addRequests = add.map((newAssignment) =>
      this.addUserUserGroupAssignment(newAssignment)
    )
    const deleteRequests = del.map((assignmentToRemove) =>
      this.removeUserUserGroupAssignment(assignmentToRemove)
    )
    await Promise.all([...addRequests, ...deleteRequests])
  }

  private addUserUserGroupAssignment(
    assignment: UserGroupAssignment
  ): Promise<void> {
    return AdminUserGroups.SaveUserAssignment({
      UserID: assignment.UserID,
      UserGroupID: assignment.UserGroupID,
    })
  }

  private removeUserUserGroupAssignment(
    assignment: UserGroupAssignment
  ): Promise<void> {
    return AdminUserGroups.DeleteUserAssignment(
      assignment.UserGroupID,
      assignment.UserID
    )
  }

  async getUserGroups(
    unusedOrgId: string, // we're adhering to an interface initially made for buyers/suppliers which additionally need an ID (admins don't)
    options: ListArgs
  ): Promise<ListPage<UserGroup>> {
    return await AdminUserGroups.List(options as any)
  }

  async listUserAssignments(
    userID: string,
    unusedOrgId: string // we're adhering to an interface initially made for buyers/suppliers which additionally need an ID (admins don't)
  ): Promise<ListPage<UserGroupAssignment>> {
    return await AdminUserGroups.ListUserAssignments({ userID })
  }
}
