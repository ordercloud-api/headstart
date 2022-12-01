using System;
using System.Threading.Tasks;
using Headstart.Common.Models;
using OrderCloud.SDK;

namespace Headstart.Common.Services
{
    public class MockCreditCardProcessor : IHSCreditCardProcessor
    {
        private readonly IOrderCloudClient orderCloudClient;

        public MockCreditCardProcessor(IOrderCloudClient orderCloudClient)
        {
            this.orderCloudClient = orderCloudClient;
        }

        public async Task<HSPayment> AuthWithoutCapture(HSPayment payment, HSBuyerCreditCard cc, HSOrder order, CCPayment ccPayment, string userToken, decimal ccAmount)
        {
            payment = await orderCloudClient.Payments.PatchAsync<HSPayment>(OrderDirection.Incoming, order.ID, payment.ID, new PartialPayment { Accepted = true, Amount = ccAmount });
            return await orderCloudClient.Payments.CreateTransactionAsync<HSPayment>(OrderDirection.Incoming, order.ID, payment.ID, CreateMockTransaction(payment, "CreditCard"));
        }

        public async Task Capture(HSOrder order, HSPayment payment, HSPaymentTransaction transaction)
        {
            await orderCloudClient.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, CreateMockTransaction(payment, "CreditCardCapture"));
        }

        public async Task<CCInquiryResult> Inquire(HSOrder order, HSPaymentTransaction creditCardPaymentTransaction)
        {
            return await Task.FromResult(new CCInquiryResult
            {
                CanVoid = true,
                CanRefund = false,
                PendingCapture = false,
                CaptureDate = order.DateCreated.ToString(),
            });
        }

        public async Task<HSBuyerCreditCard> MeTokenize(CCToken card, CurrencyCode userCurrency)
        {
            return await Task.FromResult(new HSBuyerCreditCard()
            {
                CardType = "Mock",
                CardholderName = "Mock Card",
                ExpirationDate = DateTime.Now.AddYears(1),
                PartialAccountNumber = "1234",
                Token = "MockToken",
                Editable = true,
            });
        }

        public async Task Refund(HSOrder order, HSPayment payment, HSPaymentTransaction paymentTransaction, decimal refundAmount, string orderReturnId = "")
        {
            var refundPayment = new HSPayment() { Amount = refundAmount * -1 };
            var newPaymentTransaction = CreateMockTransaction(refundPayment, "CreditCardRefund");
            if (string.IsNullOrEmpty(orderReturnId))
            {
                // // This refund isn't part of an order return so just create a new transaction on the existing payment
                await orderCloudClient.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, newPaymentTransaction);
            }
            else
            {
                // for order returns we must create a payment with a negative amount equal to the OrderReturn.RefundAmount for the order return to complete automatically
                payment = await orderCloudClient.Payments.CreateAsync<HSPayment>(OrderDirection.Incoming, order.ID, new HSPayment
                {
                    Type = PaymentType.CreditCard,
                    Amount = refundAmount * -1,
                    Accepted = true,
                    OrderReturnID = orderReturnId,
                    xp = payment.xp,
                });
                await orderCloudClient.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, newPaymentTransaction);
            }
        }

        public async Task<HSCreditCard> Tokenize(CCToken card, CurrencyCode userCurrency)
        {
            return await Task.FromResult(new HSCreditCard()
            {
                CardType = "Mock",
                CardholderName = "Mock Card",
                ExpirationDate = DateTime.Now.AddYears(1),
                PartialAccountNumber = "1234",
                Token = "MockToken",
            });
        }

        public async Task VoidAuthorization(HSOrder order, HSPayment payment, HSPaymentTransaction paymentTransaction, decimal? refundAmount = null, string orderReturnId = null)
        {
            var voidPayment = refundAmount == null ? payment : new HSPayment() { Amount = refundAmount * -1 };
            var newPaymentTransaction = CreateMockTransaction(voidPayment, "CreditCardVoidAuthorization");
            if (string.IsNullOrEmpty(orderReturnId))
            {
                // // This refund isn't part of an order return so just create a new transaction on the existing payment
                await orderCloudClient.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, newPaymentTransaction);
            }
            else
            {
                // for order returns we must create a payment with a negative amount equal to the OrderReturn.RefundAmount for the order return to complete automatically
                payment = await orderCloudClient.Payments.CreateAsync<HSPayment>(OrderDirection.Incoming, order.ID, new HSPayment
                {
                    Type = PaymentType.CreditCard,
                    Amount = refundAmount * -1,
                    Accepted = true,
                    OrderReturnID = orderReturnId,
                    xp = payment.xp,
                });
                await orderCloudClient.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, newPaymentTransaction);
            }
        }

        private PaymentTransaction CreateMockTransaction(HSPayment payment, string transactionType)
        {
            return new PaymentTransaction()
            {
                Amount = payment.Amount,
                DateExecuted = DateTime.Now,
                Succeeded = true,
                Type = transactionType,
            };
        }
    }
}
