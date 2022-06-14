using System;
using System.Threading.Tasks;
using Headstart.Common.Models;
using OrderCloud.SDK;

namespace Headstart.Common.Services
{
    public class MockCreditCardProcessor : ICreditCardProcessor
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

        public async Task Refund(HSOrderWorksheet worksheet, HSPayment payment, HSPaymentTransaction paymentTransaction, decimal refundAmount)
        {
            var refundPayment = new HSPayment() { Amount = refundAmount };
            await orderCloudClient.Payments.CreateTransactionAsync(OrderDirection.Incoming, worksheet.Order.ID, payment.ID, CreateMockTransaction(refundPayment, "CreditCardRefund"));
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

        public async Task VoidAuthorization(HSOrder order, HSPayment payment, HSPaymentTransaction paymentTransaction, decimal? refundAmount = null)
        {
            var voidPayment = refundAmount == null ? payment : new HSPayment() { Amount = refundAmount };
            await orderCloudClient.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, CreateMockTransaction(voidPayment, "CreditCardVoidAuthorization"));
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
