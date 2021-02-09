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
                        CustomRole.MPOrderAdmin,
                        CustomRole.MPShipmentAdmin,
                    }
                },
                new HSUserType {
                    UserGroupName = "Account Admin",
                    UserGroupType = UserGroupType.UserPermissions,
                    UserGroupIDSuffix = "AccountAdmin",
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.MPMeSupplierAddressAdmin,
                        CustomRole.MPMeSupplierUserAdmin,
                        CustomRole.MPSupplierUserGroupAdmin,
                        CustomRole.MPMeSupplierAdmin
                    }
                },
                new HSUserType {
                    UserGroupName = "Product Admin",
                    UserGroupType = UserGroupType.UserPermissions,
                    UserGroupIDSuffix = "ProductAdmin",
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.MPMeProductAdmin,
                    }
                },
                 new HSUserType {
                    UserGroupName = "Report Reader",
                    UserGroupType = UserGroupType.UserPermissions,
                    UserGroupIDSuffix = "ReportReader",
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.MPReportReader,
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
                        CustomRole.MPLocationPermissionAdmin,
                    }
                },
                new HSUserType {
                    UserGroupName = "Location Order Approver",
                    UserGroupType = UserGroupType.LocationPermissions,
                    UserGroupIDSuffix = UserGroupSuffix.OrderApprover.ToString(),
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.MPLocationOrderApprover,
                    }
                },
                new HSUserType {
                    UserGroupName = "Location Needs Approval",
                    UserGroupType = UserGroupType.LocationPermissions,
                    UserGroupIDSuffix = UserGroupSuffix.NeedsApproval.ToString(),
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.MPLocationNeedsApproval,
                    }
                },
                new HSUserType {
                    UserGroupName = "View All Location Orders",
                    UserGroupType = UserGroupType.LocationPermissions,
                    UserGroupIDSuffix = UserGroupSuffix.ViewAllOrders.ToString(),
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.MPLocationViewAllOrders,
                    }
                },
                new HSUserType {
                    UserGroupName = "Credit Card Admin",
                    UserGroupType = UserGroupType.LocationPermissions,
                    UserGroupIDSuffix = UserGroupSuffix.CreditCardAdmin.ToString(),
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.MPLocationCreditCardAdmin,
                    }
                },
                new HSUserType {
                    UserGroupName = "Address Admin",
                    UserGroupType = UserGroupType.LocationPermissions,
                    UserGroupIDSuffix = UserGroupSuffix.AddressAdmin.ToString(),
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.MPLocationAddressAdmin,
                    }
                },
                   new HSUserType {
                    UserGroupName = "Resale Cert Admin",
                    UserGroupType = UserGroupType.LocationPermissions,
                    UserGroupIDSuffix = UserGroupSuffix.ResaleCertAdmin.ToString(),
                    CustomRoles = new List<CustomRole>
                    {
                        CustomRole.MPLocationResaleCertAdmin,
                    }
                },
            };
        }
    }
}
