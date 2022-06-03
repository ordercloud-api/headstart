using OrderCloud.Integrations.Taxation.Interfaces;
using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class HSOrderCalculateResponse : OrderCalculateResponse<OrderCalculateResponseXp>
    {
    }

    public class OrderCalculateResponseXp
    {
        public OrderTaxCalculation TaxCalculation { get; set; }
    }
}
