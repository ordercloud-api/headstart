using OrderCloud.SDK;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using ordercloud.integrations.exchangerates;

namespace Headstart.Models
{
    public class HSBuyerLocation
    {
        public HSLocationUserGroup UserGroup { get; set; } = new HSLocationUserGroup();

        public HSAddressBuyer Address { get; set; } = new HSAddressBuyer();
    }

    public class HSUserGroup : UserGroup<UserGroupXp>, IHSObject { }

    public class UserGroupXp
    {
        public string Type { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;
    }

    public class HSLocationUserGroup : UserGroup<HSLocationUserGroupXp>, IHSObject { }

    public class HSLocationUserGroupXp
    {
        public string Type { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencySymbol? Currency { get; set; }

        public string Country { get; set; } = string.Empty;

        public List<string> CatalogAssignments { get; set; } = new List<string>();
    }
}