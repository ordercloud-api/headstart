using Headstart.Common.Services;
using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class HSOrderCalculateResponse : OrderCalculateResponse<OrderCalculateResponseXp, HSLineItemOverride>
    {
    }

    public class OrderCalculateResponseXp
    {
        public OrderTaxCalculation TaxCalculation { get; set; }
    }

    public class HSLineItemOverride : LineItemOverride<HSAdHocProduct> { }

    public class HSAdHocProduct : AdHocProduct<AdHocProductXp> { }

    public class AdHocProductXp { }
}
