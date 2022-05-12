using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using ordercloud.integrations.library;

namespace Headstart.Models.Misc
{
    
	public class HSUserType
	{
		public string UserGroupIDSuffix { get; set; }
		public string UserGroupName { get; set; }
		public UserGroupType UserGroupType { get; set; }
		public List<CustomRole> CustomRoles { get; set; }
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