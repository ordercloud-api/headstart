using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrderCloud.Integrations.Library.Models;
using OrderCloud.SDK;

namespace Headstart.Models
{
    public class HSBuyerLocation
    {
        public HSLocationUserGroup UserGroup { get; set; }

        public HSAddressBuyer Address { get; set; }
    }

    public class HSUserGroup : UserGroup<UserGroupXp>, IHSObject
    {
    }

    public class UserGroupXp
    {
        public string Type { get; set; }

        public string Role { get; set; }
    }

    public class HSLocationUserGroup : UserGroup<HSLocationUserGroupXp>, IHSObject
    {
    }

    public class HSLocationUserGroupXp
    {
        public string Type { get; set; }

        public string Role { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencyCode? Currency { get; set; } = null;

        public string Country { get; set; }

        public List<string> CatalogAssignments { get; set; }
    }
}
