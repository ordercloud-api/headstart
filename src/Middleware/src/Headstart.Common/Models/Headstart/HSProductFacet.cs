using System.Collections.Generic;
using OrderCloud.SDK;

namespace Headstart.Models
{
	public class HSProductFacet : ProductFacet<ProductFacetXp>, IHSObject
	{
	}

	public class ProductFacetXp
	{
		public IEnumerable<string> Options { get; set; }

		public string ParentID { get; set; }
	}
}
