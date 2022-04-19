using OrderCloud.SDK;
using ordercloud.integrations.library;

namespace Headstart.Common.Models.Headstart
{
	public class HsOrderCalculateResponse : OrderCalculateResponse<OrderCalculateResponseXp>
	{
	}

	public class OrderCalculateResponseXp
	{
		public OrderTaxCalculation TaxCalculation { get; set; } = new OrderTaxCalculation();
	}
}