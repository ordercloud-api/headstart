using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using Headstart.Common.Models.Misc;

namespace Headstart.Common.Models.Headstart
{
	public class HsUserType
	{
		public string UserGroupIdSuffix { get; set; } = string.Empty;

		public string UserGroupName { get; set; } = string.Empty;

		public UserGroupType UserGroupType { get; set; }

		public List<CustomRole> CustomRoles { get; set; } = new List<CustomRole>();
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum UserGroupType
	{
		UserPermissions,
		BuyerLocation,
		LocationPermissions
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum UserGroupSuffix
	{
		OrderApprover,
		PermissionAdmin,
		NeedsApproval,
		ViewAllOrders,
		CreditCardAdmin,
		AddressAdmin
	}
}