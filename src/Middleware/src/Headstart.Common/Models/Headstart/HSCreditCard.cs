using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace Headstart.Models.Headstart
{
	[SwaggerModel]
	public class HSCreditCard: CreditCard<HSCreditCardXP>
	{

	}

	[SwaggerModel]
	public class HSBuyerCreditCard : BuyerCreditCard<HSCreditCardXP>
	{

	}

	[SwaggerModel]
	public class HSCreditCardXP
	{
		[Required]
		public Address CCBillingAddress { get; set; }
	}
}
