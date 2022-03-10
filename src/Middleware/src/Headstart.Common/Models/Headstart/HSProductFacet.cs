using OrderCloud.SDK;
using System.Collections.Generic;

namespace Headstart.Common.Models.Headstart
{
	public class HsProductFacet : ProductFacet<ProductFacetXp>
	{
		public string Id { get; set; } = string.Empty;
	}

	public class ProductFacetXp
	{
		public IEnumerable<string> Options { get; set; } = new List<string>();
		public string ParentId { get; set; } = string.Empty;
	}
}