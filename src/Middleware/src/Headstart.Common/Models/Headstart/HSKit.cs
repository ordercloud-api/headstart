using Headstart.Common.Services.CMS.Models;
using Headstart.Models;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System.Collections.Generic;

namespace Headstart.Models.Headstart
{
    [SwaggerModel]
    public class HSMeKitProduct
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public HSMeProduct Product { get; set; }
        public IList<Asset> Images { get; set; }
        public IList<Asset> Attachments { get; set; }
        public HSMeKitProductAssignment ProductAssignments { get; set; }
    }

    [SwaggerModel]
    public class HSKitProduct
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public Product Product { get; set; }
        public IList<Asset> Images { get; set; }
        public IList<Asset> Attachments { get; set; }
        public HSKitProductAssignment ProductAssignments { get; set; }
    }

    [SwaggerModel]
    public class HSKitProductAssignment
    {
        public IList<HSProductInKit> ProductsInKit { get; set; }
    }

    [SwaggerModel]
    public class HSMeKitProductAssignment
    {
        public IList<HSMeProductInKit> ProductsInKit { get; set; }
    }

    [SwaggerModel]
    public class HSProductInKit
    {
        public string ID { get; set; }
        public int? MinQty { get; set; }
        public int? MaxQty { get; set; }
        public bool Static { get; set; }
        public bool Optional { get; set; }
        public string SpecCombo { get; set; }
        public IList<Variant> Variants { get; set; }
        public IList<Spec> Specs { get; set; }
        public HSProduct Product { get; set; }
        public IList<Asset> Images { get; set; }
        public IList<Asset> Attachments { get; set; }
    }

    [SwaggerModel]
    public class HSMeProductInKit
    {
        public string ID { get; set; }
        public int? MinQty { get; set; }
        public int? MaxQty { get; set; }
        public bool Static { get; set; }
        public bool Optional { get; set; }
        public string SpecCombo { get; set; }
        public IList<Variant> Variants { get; set; }
        public IList<Spec> Specs { get; set; }
        public HSMeProduct Product { get; set; }
        public IList<Asset> Images { get; set; }
        public IList<Asset> Attachments { get; set; }
    }

}
