using OrderCloud.SDK;
using ordercloud.integrations.cardconnect;

namespace Headstart.Common.Models.Headstart
{
	public class HsPayment : Payment<PaymentXp, HsPaymentTransaction>
	{
	}

	public class HsPaymentTransaction : PaymentTransaction<TransactionXP>
	{
	}

	public class PaymentXp
	{
		public string PartialAccountNumber { get; set; } = string.Empty;

		public string CardType { get; set; } = string.Empty;
	}


	public class TransactionXP
	{
		public CardConnectAuthorizationResponse CardConnectResponse { get; set; } = new CardConnectAuthorizationResponse();

		public RMADetails RMADetails { get; set; } = new RMADetails();
	}


	public class RMADetails
	{
		public string OrderRMANumber { get; set; } = string.Empty;

		public string RefundComment { get; set; } = string.Empty;

		public string FromSupplierId { get; set; } = string.Empty;

		public string FromUserId { get; set; } = string.Empty;
	}
}