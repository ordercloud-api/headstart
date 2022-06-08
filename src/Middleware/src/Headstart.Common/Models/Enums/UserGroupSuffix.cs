using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Headstart.Common.Models
{
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
}
