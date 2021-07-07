using ordercloud.integrations.library;
using OrderCloud.SDK;
using System.Collections.Generic;

namespace Headstart.Models
{
    
    public class HSCatalog : UserGroup<CatalogXp>, IHSObject
    {
    }

    
    public class PartialHSCatalog : PartialUserGroup<CatalogXp>
    {
    }

    // potentially use this for the api later
    
    public class HSCatalogAssignment : IHSObject
    {
        // ID not used but to get marketplaceobject extension working for now
        public string ID { get; set; }
        public string LocationID { get; set; }
        public string CatalogID { get; set; }
    }

    
    public class HSCatalogAssignmentRequest
    {
        public List<string> CatalogIDs { get; set;}
    }

    
    public class CatalogXp
    {
        public string Type { get; set; } = "Catalog";
    }
}
