export interface PermissionType {
    UserGroupSuffix: string
    DisplayText: string
  }
  
  export const PermissionTypes: PermissionType[] = [
    { UserGroupSuffix: 'PermissionAdmin', DisplayText: 'Permission Admin' },
    { UserGroupSuffix: 'OrderApprover', DisplayText: 'Order Approver' },
    { UserGroupSuffix: 'NeedsApproval', DisplayText: 'Needs Approval' },
    { UserGroupSuffix: 'ViewAllOrders', DisplayText: 'View All Orders' },
    { UserGroupSuffix: 'CreditCardAdmin', DisplayText: 'Credit Card Admin' },
    { UserGroupSuffix: 'AddressAdmin', DisplayText: 'Address Admin' },
  ]