using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Headstart.Common.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum RecordType
    {
        HydratedProduct,
        Product,
        PriceSchedule,
        Spec,
        SpecOption,
        SpecProductAssignment,
        ProductFacet,
        Buyer,
        User,
        UserGroup,
        Address,
        CostCenter,
        UserGroupAssignment,
        AddressAssignment,
        CatalogAssignment,
        Catalog,
        Supplier,
        Order,
        TemplateProductFlat,
    }
}
