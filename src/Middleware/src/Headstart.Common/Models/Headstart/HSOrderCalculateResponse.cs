using OrderCloud.Integrations.Taxation.Interfaces;
using OrderCloud.SDK;

namespace Headstart.Models.Headstart
{
    public class HSOrderCalculateResponse : OrderCalculateResponse<OrderCalculateResponseXp>
    {
    }

    public class OrderCalculateResponseXp
    {
        public OrderTaxCalculation TaxCalculation { get; set; }
    }
}
