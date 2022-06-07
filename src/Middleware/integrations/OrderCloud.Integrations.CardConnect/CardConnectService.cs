using System;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Headstart.Common.Services;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Alerts;
using OrderCloud.Integrations.CardConnect.Mappers;
using OrderCloud.Integrations.CardConnect.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.CardConnect
{
    public class CardConnectService : ICreditCardProcessor
    {
        private readonly CardConnectConfig cardConnectConfig;
        private readonly ICardConnectClient cardConnectClient;
        private readonly IOrderCloudClient orderCloudClient;
        private readonly ISupportAlertService supportAlertsService;

        public CardConnectService(CardConnectConfig cardConnectConfig, ICardConnectClient cardConnectClient, IOrderCloudClient orderCloudClient, ISupportAlertService supportAlertsService)
        {
            this.cardConnectConfig = cardConnectConfig;
            this.cardConnectClient = cardConnectClient;
            this.orderCloudClient = orderCloudClient;
            this.supportAlertsService = supportAlertsService;
        }

        public async Task<HSPayment> AuthWithoutCapture(HSPayment payment, HSBuyerCreditCard buyerCreditCard, HSOrder order, CCPayment ccPayment, string userToken, decimal ccAmount)
        {
            try
            {
                var call = await cardConnectClient.AuthWithoutCapture(CardConnectMapper.Map(buyerCreditCard, order, ccPayment, GetMerchantID(ccPayment.Currency), ccAmount));
                payment = await orderCloudClient.Payments.PatchAsync<HSPayment>(OrderDirection.Incoming, order.ID, payment.ID, new PartialPayment { Accepted = true, Amount = ccAmount });
                return await orderCloudClient.Payments.CreateTransactionAsync<HSPayment>(OrderDirection.Incoming, order.ID, payment.ID, CardConnectMapper.Map(payment, call));
            }
            catch (CreditCardAuthorizationException ex)
            {
                payment = await orderCloudClient.Payments.PatchAsync<HSPayment>(OrderDirection.Incoming, order.ID, payment.ID, new PartialPayment { Accepted = false, Amount = ccAmount });
                await orderCloudClient.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, CardConnectMapper.Map(payment, ex.Response));
                throw new CatalystBaseException($"CreditCardAuth.{ex.ApiError.ErrorCode}", ex.ApiError.Message, ex.Response);
            }
        }

        public async Task Capture(HSOrder order, HSPayment payment, HSPaymentTransaction transaction)
        {
            try
            {
                var response = await cardConnectClient.Capture(new CardConnectCaptureRequest
                {
                    merchid = transaction.xp.CCTransactionResult.merchid,
                    retref = transaction.xp.CCTransactionResult.TransactionID,
                    currency = order.xp.Currency.ToString(),
                });
                await orderCloudClient.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, CardConnectMapper.Map(payment, response));
            }
            catch (CardConnectCaptureException ex)
            {
                await orderCloudClient.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, CardConnectMapper.Map(payment, ex.Response));
                throw new CatalystBaseException($"CreditCardAuth.{ex.ApiError.ErrorCode}", ex.ApiError.Message, ex.Response);
            }
        }

        public async Task<CCInquiryResult> Inquire(HSOrder order, HSPaymentTransaction creditCardPaymentTransaction)
        {
            var inquiry = await cardConnectClient.Inquire(new CardConnectInquireRequest
            {
                merchid = creditCardPaymentTransaction.xp.CCTransactionResult.merchid,
                orderid = order.ID,
                set = "1",
                currency = order.xp.Currency.ToString(),
                retref = creditCardPaymentTransaction.xp.CCTransactionResult.TransactionID,
            });

            return new CCInquiryResult
            {
                CanVoid = inquiry.voidable == "Y",
                CanRefund = inquiry.voidable == "N",
                PendingCapture = inquiry.voidable == "Y" && inquiry.setlstat == "Queued for Capture",
                CaptureDate = inquiry.capturedate,
            };
        }

        public async Task VoidAuthorization(HSOrder order, HSPayment payment, HSPaymentTransaction paymentTransaction, decimal? refundAmount = null)
        {
            var voidPayment = refundAmount == null ? payment : new HSPayment() { Amount = refundAmount };
            try
            {
                var response = await cardConnectClient.VoidAuthorization(new CardConnectVoidRequest
                {
                    currency = order.xp.Currency.ToString(),
                    merchid = paymentTransaction.xp.CCTransactionResult.merchid,
                    retref = paymentTransaction.xp.CCTransactionResult.TransactionID,
                    amount = refundAmount?.ToString("F2"),
                });

                await orderCloudClient.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, CardConnectMapper.Map(voidPayment, response));
            }
            catch (CreditCardVoidException ex)
            {
                await supportAlertsService.VoidAuthorizationFailed(payment, paymentTransaction.ID, order, ex.ApiError);
                await orderCloudClient.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, CardConnectMapper.Map(voidPayment, ex.Response));
                throw new CatalystBaseException(new ApiError
                {
                    ErrorCode = "Payment.FailedToVoidAuthorization",
                    Message = ex.ApiError.Message,
                });
            }
        }

        public async Task Refund(HSOrderWorksheet worksheet, HSPayment payment, HSPaymentTransaction paymentTransaction, decimal refundAmount)
        {
            try
            {
                var response = await cardConnectClient.Refund(new CardConnectRefundRequest
                {
                    currency = worksheet.Order.xp.Currency.ToString(),
                    merchid = paymentTransaction.xp.CCTransactionResult.merchid,
                    retref = paymentTransaction.xp.CCTransactionResult.TransactionID,
                    amount = refundAmount.ToString("F2"),
                });

                var newPaymentTransaction = CardConnectMapper.Map(new HSPayment() { Amount = response.amount }, response);
                await orderCloudClient.Payments.CreateTransactionAsync(OrderDirection.Incoming, worksheet.Order.ID, payment.ID, newPaymentTransaction);
            }
            catch (CreditCardRefundException ex)
            {
                throw new CatalystBaseException(new ApiError
                {
                    ErrorCode = "Payment.FailedToRefund",
                    Message = ex.ApiError.Message,
                });
            }
        }

        public async Task<HSBuyerCreditCard> MeTokenize(CCToken card, CurrencyCode userCurrency)
        {
            var auth = await cardConnectClient.Tokenize(CardConnectMapper.Map(card, userCurrency.ToString()));
            return BuyerCreditCardMapper.Map(card, auth);
        }

        public async Task<HSCreditCard> Tokenize(CCToken card, CurrencyCode userCurrency)
        {
            var auth = await cardConnectClient.Tokenize(CardConnectMapper.Map(card, userCurrency.ToString()));
            return CreditCardMapper.Map(card, auth);
        }

        private string GetMerchantID(CurrencyCode userCurrency)
        {
            return userCurrency switch
            {
                CurrencyCode.USD => cardConnectConfig.UsdMerchantID,
                CurrencyCode.CAD => cardConnectConfig.CadMerchantID,
                _ => cardConnectConfig.EurMerchantID
            };
        }

        private string GetMerchantID(string userCurrency)
        {
            Enum.TryParse(userCurrency, out CurrencyCode currency);
            return GetMerchantID(currency);
        }
    }
}
