using OrderCloud.SDK;
using System.Collections.Generic;
using Headstart.Common.Models.Base;

namespace Headstart.Common.Models.Headstart
{
	public class HsCatalog : UserGroup<CatalogXp>
	{
		public string Id { get; set; } = string.Empty;
	}

	public class PartialHSCatalog : PartialUserGroup<CatalogXp>
	{
	}

	public class HsCatalogAssignment : HsBaseObject
	{
		public string LocationId { get; set; } = string.Empty;

		public string CatalogId { get; set; } = string.Empty;
	}

	public class HsCatalogAssignmentRequest
	{
		public List<string> CatalogIDs { get; set; } = new List<string>();
	}

	public class CatalogXp
	{
		public string Type { get; set; } = @"Catalog";
	}
}