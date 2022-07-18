import {
  ListPage,
  MeUser,
  UserGroup,
  UserGroupAssignment,
} from 'ordercloud-javascript-sdk'
import { ListArgs } from '@ordercloud/headstart-sdk'

export interface IUserPermissionsService {
  getUserGroups(orgID: string, options: ListArgs): Promise<ListPage<UserGroup>>
  listUserAssignments(
    userID: string,
    orgID: string
  ): Promise<ListPage<UserGroupAssignment>>
  getParentResourceID(): Promise<string>
  updateUserUserGroupAssignments(
    orgID: string,
    add: UserGroupAssignment[],
    del: UserGroupAssignment[],
    shouldSyncUserCatalogAssignments: boolean
  ): Promise<void>
}

export interface PermissionType {
  UserGroupSuffix: string
  DisplayText: string
}

export interface HSRole {
  RoleName: string
  OrderCloudRoles: string[]
}

export interface UserType {
  Name: string
  HSRoles: HSRole[]
}

export interface UserContext {
  Me: MeUser
  UserRoles: string[]
  UserType: string
}

export interface AssignmentsToAddUpdate {
  UserGroupType: string
  Assignments: UserGroupAssignment[]
}

export const SELLER = 'SELLER'
export type SELLER = typeof SELLER

export const SUPPLIER = 'SUPPLIER'
export type SUPPLIER = typeof SUPPLIER

export type OrderCloudUserType = SELLER | SUPPLIER
