using System.Collections.Generic;
using OrderCloud.SDK;

namespace Headstart.Models
{
    public class HSCatalog : UserGroup<CatalogXp>, IHSObject
    {
    }

    // potentially use this for the api later
    public class HSCatalogAssignment
    {
        public string LocationID { get; set; }

        public string CatalogID { get; set; }
    }

    public class HSCatalogAssignmentRequest
    {
        public List<string> CatalogIDs { get; set; }
    }

    public class CatalogXp
    {
    }
}
