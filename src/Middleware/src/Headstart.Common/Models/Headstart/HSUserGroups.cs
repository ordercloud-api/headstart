using OrderCloud.SDK;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using ordercloud.integrations.exchangerates;

namespace Headstart.Common.Models.Headstart
{
	public class HsBuyerLocation
	{
		public HsLocationUserGroup UserGroup { get; set; } = new HsLocationUserGroup();

		public HsAddressBuyer Address { get; set; } = new HsAddressBuyer();
	}

	public class HsUserGroup : UserGroup<UserGroupXp>
	{
		public string Id { get; set; } = string.Empty;
	}

	public class UserGroupXp
	{
		public string Type { get; set; } = string.Empty;

		public string Role { get; set; } = string.Empty;
	}

	public class HsLocationUserGroup : UserGroup<HsLocationUserGroupXp>
	{
		public string Id { get; set; } = string.Empty;
	}

	public class HsLocationUserGroupXp
	{
		public string Type { get; set; } = string.Empty;

		public string Role { get; set; } = string.Empty;

		[JsonConverter(typeof(StringEnumConverter))]
		public CurrencySymbol? Currency { get; set; }

		public string Country { get; set; } = string.Empty;

		public List<string> CatalogAssignments { get; set; } = new List<string>();
	}
}