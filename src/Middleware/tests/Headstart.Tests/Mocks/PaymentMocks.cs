using OrderCloud.SDK;
using System.Collections.Generic;
using Headstart.Common.Models.Headstart;

namespace Headstart.Tests.Mocks
{
	public static class PaymentMocks
	{
		public static List<HsPayment> Payments(params HsPayment[] payments)
		{
			return new List<HsPayment>(payments);
		}

		public static ListPage<HsPayment> EmptyPaymentsList()
		{
			var items = new List<HsPayment>();
			return new ListPage<HsPayment>
			{
				Items = items
			};
		}

		public static ListPage<HsPayment> PaymentList(params HsPayment[] payments)
		{
			return new ListPage<HsPayment>
			{
				Items = new List<HsPayment>(payments)
			};
		}

		public static HsPayment CCPayment(string creditCardID, decimal? amount = null, string id = "mockCCPaymentID", bool accepted = true)
		{
			return new HsPayment
			{
				ID = id,
				Type = PaymentType.CreditCard,
				CreditCardID = creditCardID,
				Amount = amount,
				Accepted = accepted
			};
		}

		public static HsPayment POPayment(decimal? amount = null, string id = "mockPoPaymentID")
		{
			return new HsPayment
			{
				ID = id,
				Type = PaymentType.PurchaseOrder,
				Amount = amount
			};
		}

		public static HsPayment SpendingAccountPayment(decimal? amount = null, string id = "mockSpendingAccountID")
		{
			return new HsPayment
			{
				ID = id,
				Type = PaymentType.SpendingAccount,
				Amount = amount
			};
		}
	}
}
