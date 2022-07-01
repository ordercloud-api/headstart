export interface UserGroupDisplayText {
  Title: string
  InfoText: string
  ConfirmText: string
  Column2: string
}

const MapToUserGroupDisplayText = {
  UserPermissions: {
    Title: 'ADMIN.LOCATIONS.USER_PERMISSIONS',
    InfoText: 'ADMIN.LOCATIONS.SELECT_PERMISSIONS_TO_GRANT',
    ConfirmText: 'ADMIN.LOCATIONS.CONFIRM_PERMISSION_CHANGES',
    Column2: 'ADMIN.LOCATIONS.PERMISSION',
  },
  BuyerLocation: {
    Title: 'ADMIN.LOCATIONS.LOCATIONS',
    InfoText: `ADMIN.LOCATIONS.SELECT_USERS_LOCATIONS`,
    ConfirmText: 'ADMIN.LOCATIONS.CONFIRM_PERMISSION_CHANGES',
    Column2: 'ADMIN.LOCATIONS.LOCATION',
  },
}

export const GetDisplayText = (userGroupType: string): UserGroupDisplayText => {
  return MapToUserGroupDisplayText[userGroupType] as UserGroupDisplayText
}

export const UserGroupTypes = {
  UserPermissions: 'UserPermissions',
  BuyerLocation: 'BuyerLocation',
}
