using Headstart.Common.Models;
using System;
using System.Collections.Generic;
using System.Text;
using Headstart.Models.Misc;

namespace Headstart.Common.Constants
{
    public static class HSUserTypes
    {
        public static List<HSUserType> Supplier() {
            return new List<HSUserType>()
            {
                new HSUserType {
                    UserGroupName = "Order Admin",
                    UserGroupType = UserGroupType.UserPermissions,
                    UserGroupIDSuffix = "OrderAdmin",
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.HSOrderAdmin,
                        CustomRole.HSShipmentAdmin,
                    }
                },
                new HSUserType {
                    UserGroupName = "Account Admin",
                    UserGroupType = UserGroupType.UserPermissions,
                    UserGroupIDSuffix = "AccountAdmin",
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.HSMeSupplierAddressAdmin,
                        CustomRole.HSMeSupplierUserAdmin,
                        CustomRole.HSSupplierUserGroupAdmin,
                        CustomRole.HSMeSupplierAdmin
                    }
                },
                new HSUserType {
                    UserGroupName = "Product Admin",
                    UserGroupType = UserGroupType.UserPermissions,
                    UserGroupIDSuffix = "ProductAdmin",
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.HSMeProductAdmin,
                    }
                },
                 new HSUserType {
                    UserGroupName = "Report Reader",
                    UserGroupType = UserGroupType.UserPermissions,
                    UserGroupIDSuffix = "ReportReader",
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.HSReportReader,
                    }
                },
            };
                
        }

        public static List<HSUserType> BuyerLocation()
        {
            return new List<HSUserType>()
            {
                 new HSUserType {
                    UserGroupName = "Location Permission Admin",
                    UserGroupType = UserGroupType.LocationPermissions,
                    UserGroupIDSuffix = UserGroupSuffix.PermissionAdmin.ToString(),
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.HSLocationPermissionAdmin,
                    }
                },
                new HSUserType {
                    UserGroupName = "Location Order Approver",
                    UserGroupType = UserGroupType.LocationPermissions,
                    UserGroupIDSuffix = UserGroupSuffix.OrderApprover.ToString(),
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.HSLocationOrderApprover,
                    }
                },
                new HSUserType {
                    UserGroupName = "Location Needs Approval",
                    UserGroupType = UserGroupType.LocationPermissions,
                    UserGroupIDSuffix = UserGroupSuffix.NeedsApproval.ToString(),
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.HSLocationNeedsApproval,
                    }
                },
                new HSUserType {
                    UserGroupName = "View All Location Orders",
                    UserGroupType = UserGroupType.LocationPermissions,
                    UserGroupIDSuffix = UserGroupSuffix.ViewAllOrders.ToString(),
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.HSLocationViewAllOrders,
                    }
                },
                new HSUserType {
                    UserGroupName = "Credit Card Admin",
                    UserGroupType = UserGroupType.LocationPermissions,
                    UserGroupIDSuffix = UserGroupSuffix.CreditCardAdmin.ToString(),
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.HSLocationCreditCardAdmin,
                    }
                },
                new HSUserType {
                    UserGroupName = "Address Admin",
                    UserGroupType = UserGroupType.LocationPermissions,
                    UserGroupIDSuffix = UserGroupSuffix.AddressAdmin.ToString(),
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.HSLocationAddressAdmin,
                    }
                },
            };
        }
    }
}
