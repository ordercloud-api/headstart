using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common;
using Headstart.Common.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.CardConnect;
using OrderCloud.Integrations.CardConnect.Mappers;
using OrderCloud.Integrations.CardConnect.Models;
using OrderCloud.SDK;

namespace Headstart.Jobs
{
    public class PaymentCaptureJob : BaseTimerJob
    {
        private readonly AppSettings settings;
        private readonly IOrderCloudClient oc;
        private readonly IOrderCloudIntegrationsCardConnectService cardConnect;
        private readonly IRMACommand rmaCommand;

        public PaymentCaptureJob(AppSettings settings, IOrderCloudClient oc, IOrderCloudIntegrationsCardConnectService cardConnect, IRMACommand rmaCommand)
        {
            this.settings = settings;
            this.oc = oc;
            this.cardConnect = cardConnect;
            this.rmaCommand = rmaCommand;
        }

        protected override bool ShouldRun => settings.JobSettings.ShouldCaptureCreditCardPayments;

        protected override async Task ProcessJob()
        {
            var filters = new Dictionary<string, object>
            {
                ["DateSubmitted"] = $">{settings.JobSettings.CaptureCreditCardsAfterDate}", // limiting scope of orders until all past orders have been updated with xp.IsPaymentCaptured=true
                ["xp.PaymentMethod"] = "Credit Card",
                ["xp.IsPaymentCaptured"] = "false|!*", // TODO: once this is in place for a week set xp.IsPaymentCaptured to true on all past orders so this filter can be more performant
                ["IsSubmitted"] = true,
                ["xp.SubmittedOrderStatus"] = "!Canceled",
            };

            var orders = await oc.Orders.ListAllAsync<HSOrder>(OrderDirection.Incoming, filters: filters);
            _logger.LogInformation($"Found {orders.Count} orders to process");

            await Throttler.RunAsync(orders, 100, 5, ProcessSingleOrder);
        }

        private async Task ProcessSingleOrder(HSOrder order)
        {
            try
            {
                var rmaList = await rmaCommand.ListRMAsByOrderID(order.ID, CommerceRole.Seller, new MeUser { });
                if (rmaList.Items.Any(x => x.Status == RMAStatus.Complete))
                {
                    LogSkip($"{order.ID} has been refunded - RMA process handles authorizing new partial amount if necessary");
                    await oc.Orders.PatchAsync(OrderDirection.Incoming, order.ID, new PartialOrder { xp = new { IsPaymentCaptured = true } });
                    return;
                }

                var payment = await GetValidPaymentAsync(order);
                var transaction = GetValidTransaction(order.ID, payment);
                if (await HasBeenCapturedPreviouslyAsync(order, transaction))
                {
                    LogSkip($"{order.ID} has already been captured");
                    await oc.Orders.PatchAsync(OrderDirection.Incoming, order.ID, new PartialOrder { xp = new { IsPaymentCaptured = true } });
                }
                else
                {
                    await CapturePaymentAsync(order, payment, transaction);
                    await oc.Orders.PatchAsync(OrderDirection.Incoming, order.ID, new PartialOrder { xp = new { IsPaymentCaptured = true } });
                    LogSuccess(order.ID);
                }
            }
            catch (OrderCloudException ex)
            {
                LogFailure($"{ex.InnerException.Message} {JsonConvert.SerializeObject(ex.Errors)}. OrderID: {order.ID}");
            }
            catch (PaymentCaptureJobException ex)
            {
                LogFailure(ex.Message);
            }
            catch (Exception ex)
            {
                LogFailure($"{ex.Message}. OrderID: {order.ID}");
            }
        }

        private async Task<HSPayment> GetValidPaymentAsync(HSOrder order)
        {
            if (order.xp.Currency == null)
            {
                throw new PaymentCaptureJobException("Order.xp.Currency is null", order.ID);
            }

            var paymentList = await oc.Payments.ListAsync<HSPayment>(OrderDirection.Incoming, order.ID, filters: new { Accepted = true, Type = "CreditCard" });
            if (paymentList.Items.Count == 0)
            {
                throw new PaymentCaptureJobException("No credit card payment on the order where Accepted=true", order.ID);
            }

            return paymentList.Items[0];
        }

        private HSPaymentTransaction GetValidTransaction(string orderID, HSPayment payment)
        {
            var ordered = payment.Transactions.OrderBy(x => x.DateExecuted);
            var transaction = payment.Transactions
                                    .OrderBy(x => x.DateExecuted)
                                    .LastOrDefault(x => x.Type == "CreditCard" && x.Succeeded);
            if (transaction == null)
            {
                throw new PaymentCaptureJobException("No valid payment authorization on the order", orderID, payment.ID);
            }

            if (transaction?.xp?.CardConnectResponse == null)
            {
                throw new PaymentCaptureJobException("Missing transaction.xp.CardConnectResponse", orderID, payment.ID, transaction.ID);
            }

            var authHasBeenVoided = payment.Transactions.Any(t =>
                                            t.Type == "CreditCardVoidAuthorization" &&
                                            t.Succeeded &&
                                            t.xp?.CardConnectResponse?.retref == transaction.xp?.CardConnectResponse?.retref);
            if (authHasBeenVoided)
            {
                throw new PaymentCaptureJobException("Payment authorization has been voided", orderID, payment.ID, transaction.ID);
            }

            return transaction;
        }

        private async Task<bool> HasBeenCapturedPreviouslyAsync(HSOrder order, HSPaymentTransaction transaction)
        {
            var inquire = await cardConnect.Inquire(new CardConnectInquireRequest
            {
                merchid = transaction.xp.CardConnectResponse.merchid,
                orderid = order.ID,
                set = "1",
                currency = order.xp.Currency.ToString(),
                retref = transaction.xp.CardConnectResponse.retref,
            });
            return !string.IsNullOrEmpty(inquire.capturedate);
        }

        private async Task CapturePaymentAsync(HSOrder order, HSPayment payment, HSPaymentTransaction transaction)
        {
            try
            {
                var response = await cardConnect.Capture(new CardConnectCaptureRequest
                {
                    merchid = transaction.xp.CardConnectResponse.merchid,
                    retref = transaction.xp.CardConnectResponse.retref,
                    currency = order.xp.Currency.ToString(),
                });
                await oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, CardConnectMapper.Map(payment, response));
            }
            catch (CardConnectInquireException ex)
            {
                throw new PaymentCaptureJobException($"Error inquiring payment. Message: {ex.ApiError.Message}, ErrorCode: {ex.ApiError.ErrorCode}", order.ID, payment.ID, transaction.ID);
            }
            catch (CardConnectCaptureException ex)
            {
                await oc.Payments.CreateTransactionAsync(OrderDirection.Incoming, order.ID, payment.ID, CardConnectMapper.Map(payment, ex.Response));
                throw new PaymentCaptureJobException($"Error capturing payment. Message: {ex.ApiError.Message}, ErrorCode: {ex.ApiError.ErrorCode}", order.ID, payment.ID, transaction.ID);
            }
        }
    }

    public class PaymentCaptureJobException : Exception
    {
        public PaymentCaptureJobException(string message, string orderID)
            : base($"{message}. OrderID: {orderID}")
        {
        }

        public PaymentCaptureJobException(string message, string orderID, string paymentID)
            : base($"{message}. OrderID: {orderID}. PaymentID: {paymentID}")
        {
        }

        public PaymentCaptureJobException(string message, string orderID, string paymentID, string transactionID)
            : base($"{message}. OrderID: {orderID}. PaymentID: {paymentID}. TransactionID: {transactionID}")
        {
        }
    }
}
