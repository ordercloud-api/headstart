using System.Collections.Generic;

namespace Headstart.Common.Models
{
    public class HSCatalogAssignmentRequest
    {
        /// <summary>
        /// The Catalog IDs for user group catalog assignments.
        /// </summary>
        public List<string> CatalogIDs { get; set; }
    }
}
