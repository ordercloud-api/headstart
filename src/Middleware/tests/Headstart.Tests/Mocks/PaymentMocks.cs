using Headstart.Models.Headstart;
using ordercloud.integrations.cardconnect;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Tests.Mocks
{
    public static class PaymentMocks
    {
        public static List<HSPayment> Payments(params HSPayment[] payments)
        {
            return new List<HSPayment>(payments);
        }

        public static ListPage<HSPayment> EmptyPaymentsList()
        {
            var items = new List<HSPayment>();
            return new ListPage<HSPayment>
            {
                Items = items
            };
        }

        public static ListPage<HSPayment> PaymentList(params HSPayment[] payments)
        {
            return new ListPage<HSPayment>
            {
                Items = new List<HSPayment>(payments)
            };
        }

        public static HSPayment CCPayment(string creditCardID, decimal? amount = null, string id = "mockCCPaymentID", bool accepted = true)
        {
            return new HSPayment
            {
                ID = id,
                Type = PaymentType.CreditCard,
                CreditCardID = creditCardID,
                Amount = amount,
                Accepted = accepted
            };
        }

        public static HSPayment POPayment(decimal? amount = null, string id = "mockPoPaymentID")
        {
            return new HSPayment
            {
                ID = id,
                Type = PaymentType.PurchaseOrder,
                Amount = amount
            };
        }

        public static HSPayment SpendingAccountPayment(decimal? amount = null, string id = "mockSpendingAccountID")
        {
            return new HSPayment
            {
                ID = id,
                Type = PaymentType.SpendingAccount,
                Amount = amount
            };
        }
    }
}
