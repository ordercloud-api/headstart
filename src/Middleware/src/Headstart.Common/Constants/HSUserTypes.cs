using System.Collections.Generic;
using Headstart.Common.Models.Misc;
using Headstart.Common.Models.Headstart;

namespace Headstart.Common.Constants
{
	public static class HsUserTypes
	{
		public static List<HsUserType> Supplier()
		{
			return new List<HsUserType>()
			{
				new HsUserType 
				{
					UserGroupName = @"Order Admin",
					UserGroupType = UserGroupType.UserPermissions,
					UserGroupIdSuffix = @"OrderAdmin",
					CustomRoles = new List<CustomRole>
					{
						CustomRole.HSOrderAdmin,
						CustomRole.HSShipmentAdmin,
					}
				},
				new HsUserType 
				{
					UserGroupName = @"Account Admin",
					UserGroupType = UserGroupType.UserPermissions,
					UserGroupIdSuffix = @"AccountAdmin",
					CustomRoles = new List<CustomRole>
					{
						CustomRole.HSMeSupplierAddressAdmin,
						CustomRole.HSMeSupplierUserAdmin,
						CustomRole.HSSupplierUserGroupAdmin,
						CustomRole.HSMeSupplierAdmin
					}
				},
				new HsUserType 
				{
					UserGroupName = @"Product Admin",
					UserGroupType = UserGroupType.UserPermissions,
					UserGroupIdSuffix = @"ProductAdmin",
					CustomRoles = new List<CustomRole>
					{
						CustomRole.HSMeProductAdmin,
					}
				},
				new HsUserType 
				{
					UserGroupName = @"Report Reader",
					UserGroupType = UserGroupType.UserPermissions,
					UserGroupIdSuffix = @"ReportReader",
					CustomRoles = new List<CustomRole>
					{
						CustomRole.HSReportReader,
					}
				},
			};
		}

		public static List<HsUserType> BuyerLocation()
		{
			return new List<HsUserType>()
			{
				new HsUserType 
				{
					UserGroupName = @"Location Permission Admin",
					UserGroupType = UserGroupType.LocationPermissions,
					UserGroupIdSuffix = UserGroupSuffix.PermissionAdmin.ToString(),
					CustomRoles = new List<CustomRole>
					{
						CustomRole.HSLocationPermissionAdmin,
					}
				},
				new HsUserType 
				{
					UserGroupName = @"Location Order Approver",
					UserGroupType = UserGroupType.LocationPermissions,
					UserGroupIdSuffix = UserGroupSuffix.OrderApprover.ToString(),
					CustomRoles = new List<CustomRole>
					{
						CustomRole.HSLocationOrderApprover,
					}
				},
				new HsUserType 
				{
					UserGroupName = @"Location Needs Approval",
					UserGroupType = UserGroupType.LocationPermissions,
					UserGroupIdSuffix = UserGroupSuffix.NeedsApproval.ToString(),
					CustomRoles = new List<CustomRole>
					{
						CustomRole.HSLocationNeedsApproval,
					}
				},
				new HsUserType 
				{
					UserGroupName = @"View All Location Orders",
					UserGroupType = UserGroupType.LocationPermissions,
					UserGroupIdSuffix = UserGroupSuffix.ViewAllOrders.ToString(),
					CustomRoles = new List<CustomRole>
					{
						CustomRole.HSLocationViewAllOrders,
					}
				},
				new HsUserType 
				{
					UserGroupName = @"Credit Card Admin",
					UserGroupType = UserGroupType.LocationPermissions,
					UserGroupIdSuffix = UserGroupSuffix.CreditCardAdmin.ToString(),
					CustomRoles = new List<CustomRole>
					{
						CustomRole.HSLocationCreditCardAdmin,
					}
				},
				new HsUserType 
				{
					UserGroupName = @"Address Admin",
					UserGroupType = UserGroupType.LocationPermissions,
					UserGroupIdSuffix = UserGroupSuffix.AddressAdmin.ToString(),
					CustomRoles = new List<CustomRole>
					{
						CustomRole.HSLocationAddressAdmin,
					}
				}
			};
		}
	}
}