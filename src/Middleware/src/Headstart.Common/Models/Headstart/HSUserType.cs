using System.Collections.Generic;
using Headstart.Models.Misc;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Headstart.Common.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserGroupType
    {
        UserPermissions,
        BuyerLocation,
        LocationPermissions,
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum UserGroupSuffix
    {
        OrderApprover,
        PermissionAdmin,
        NeedsApproval,
        ViewAllOrders,
        CreditCardAdmin,
        AddressAdmin,
    }

    public class HSUserType
    {
        public string UserGroupIDSuffix { get; set; }

        public string UserGroupName { get; set; }

        public UserGroupType UserGroupType { get; set; }

        public List<CustomRole> CustomRoles { get; set; }
    }
}
