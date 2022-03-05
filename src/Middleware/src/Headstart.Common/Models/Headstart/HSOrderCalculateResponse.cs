using OrderCloud.SDK;
using ordercloud.integrations.library;

namespace Headstart.Models.Headstart
{
    public class HSOrderCalculateResponse : OrderCalculateResponse<OrderCalculateResponseXp> { }

    public class OrderCalculateResponseXp
    {
        public OrderTaxCalculation TaxCalculation { get; set; } = new OrderTaxCalculation();
    }
}