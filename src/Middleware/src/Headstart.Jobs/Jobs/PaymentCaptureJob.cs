using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Headstart.Common.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.RMAs.Commands;
using OrderCloud.Integrations.RMAs.Models;
using OrderCloud.SDK;

namespace Headstart.Jobs
{
    public class PaymentCaptureJob : BaseTimerJob
    {
        private readonly AppSettings settings;
        private readonly IOrderCloudClient oc;
        private readonly IHSCreditCardProcessor creditCardService;
        private readonly IRMACommand rmaCommand;

        public PaymentCaptureJob(AppSettings settings, IOrderCloudClient oc, IHSCreditCardProcessor creditCardService, IRMACommand rmaCommand)
        {
            this.settings = settings;
            this.oc = oc;
            this.creditCardService = creditCardService;
            this.rmaCommand = rmaCommand;
        }

        protected override bool ShouldRun => settings.JobSettings.ShouldCaptureCreditCardPayments;

        protected override async Task ProcessJob()
        {
            var filters = new Dictionary<string, object>
            {
                ["xp.PaymentMethod"] = "Credit Card",
                ["xp.IsPaymentCaptured"] = "false",
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

            if (transaction?.xp?.CCTransactionResult == null)
            {
                throw new PaymentCaptureJobException("Missing transaction.xp.CardConnectResponse", orderID, payment.ID, transaction.ID);
            }

            var authHasBeenVoided = payment.Transactions.Any(t =>
                                            t.Type == "CreditCardVoidAuthorization" &&
                                            t.Succeeded &&
                                            t.xp?.CCTransactionResult?.TransactionID == transaction.xp?.CCTransactionResult?.TransactionID);
            if (authHasBeenVoided)
            {
                throw new PaymentCaptureJobException("Payment authorization has been voided", orderID, payment.ID, transaction.ID);
            }

            return transaction;
        }

        private async Task<bool> HasBeenCapturedPreviouslyAsync(HSOrder order, HSPaymentTransaction transaction)
        {
            var inquire = await creditCardService.Inquire(order, transaction);
            return !string.IsNullOrEmpty(inquire.CaptureDate);
        }

        private async Task CapturePaymentAsync(HSOrder order, HSPayment payment, HSPaymentTransaction transaction)
        {
            try
            {
                await creditCardService.Capture(order, payment, transaction);
            }
            catch (CatalystBaseException ex)
            {
                throw new PaymentCaptureJobException($"Error capturing payment. Message: {ex.Errors[0].Message}, ErrorCode: {ex.Errors[0].ErrorCode}", order.ID, payment.ID, transaction.ID);
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
