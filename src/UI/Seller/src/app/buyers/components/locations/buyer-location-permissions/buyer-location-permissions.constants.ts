import { PermissionType } from '@app-seller/shared'

export const PermissionTypes: PermissionType[] = [
  {
    UserGroupSuffix: 'PermissionAdmin',
    DisplayText: 'ADMIN.PERMISSIONS.PERMISSION_ADMIN',
  },
  {
    UserGroupSuffix: 'OrderApprover',
    DisplayText: 'ADMIN.PERMISSIONS.ORDER_APPROVER',
  },
  {
    UserGroupSuffix: 'NeedsApproval',
    DisplayText: 'ADMIN.PERMISSIONS.NEEDS_APPROVAL',
  },
  {
    UserGroupSuffix: 'ViewAllOrders',
    DisplayText: 'ADMIN.PERMISSIONS.VIEW_ALL_ORDERS',
  },
  {
    UserGroupSuffix: 'CreditCardAdmin',
    DisplayText: 'ADMIN.PERMISSIONS.CREDIT_CARD_ADMIN',
  },
  {
    UserGroupSuffix: 'AddressAdmin',
    DisplayText: 'ADMIN.PERMISSIONS.ADDRESS_ADMIN',
  },
]
