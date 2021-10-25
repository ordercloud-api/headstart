import { PermissionType } from "@app-seller/shared"

export const UserGroupTypes = {
  UserPermissions: 'UserPermissions',
  BuyerLocation: 'BuyerLocation',
}

const MapToUserGroupDisplayText = {
  UserPermissions: {
    Title: 'User Permissions',
    InfoText: 'Select which permissions to grant this user.',
    ConfirmText:
      "Please confirm that you wish to alter this user's permissions.",
    Column2: 'Permission',
  },
  BuyerLocation: {
    Title: 'Locations',
    InfoText: `Select this user's locations.`,
    ConfirmText: "Please confirm that you wish to alter this user's locations.",
    Column2: 'Location',
  },
}

export const GetDisplayText = (userGroupType: string): string => {
  return MapToUserGroupDisplayText[userGroupType]
}

export const PermissionTypes: PermissionType[] = [
  { UserGroupSuffix: 'PermissionAdmin', DisplayText: 'Permission Admin' },
  { UserGroupSuffix: 'OrderApprover', DisplayText: 'Order Approver' },
  { UserGroupSuffix: 'NeedsApproval', DisplayText: 'Needs Approval' },
  { UserGroupSuffix: 'ViewAllOrders', DisplayText: 'View All Orders' },
  { UserGroupSuffix: 'CreditCardAdmin', DisplayText: 'Credit Card Admin' },
  { UserGroupSuffix: 'AddressAdmin', DisplayText: 'Address Admin' },
]

