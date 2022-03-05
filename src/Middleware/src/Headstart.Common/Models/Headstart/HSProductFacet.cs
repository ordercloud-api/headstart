using OrderCloud.SDK;
using System.Collections.Generic;

namespace Headstart.Models
{
    public class HSProductFacet : ProductFacet<ProductFacetXp>, IHSObject { }

    public class ProductFacetXp
    {
        public IEnumerable<string> Options { get; set; } = new List<string>();
        public string ParentID { get; set; } = string.Empty;
    }
}