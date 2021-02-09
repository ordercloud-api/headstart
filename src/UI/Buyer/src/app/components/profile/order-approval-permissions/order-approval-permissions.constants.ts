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
    InfoText: 'Select which locations to which this user belongs.',
    ConfirmText: "Please confirm that you wish to alter this user's locations.",
    Column2: 'Location',
  },
}

export const GetDisplayText = (userGroupType: string): string => {
  return MapToUserGroupDisplayText[userGroupType]
}
