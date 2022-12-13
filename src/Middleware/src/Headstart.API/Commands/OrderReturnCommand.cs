using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Headstart.Common.Services;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.API.Commands
{
    public interface IOrderReturnCommand
    {
        public Task<HSOrderReturn> CompleteReturn(string orderReturnId);

        public Task<IEnumerable<LineItemReturnCalculation>> CalculateOrderReturn(string orderId, List<OrderReturnItem> itemsToReturn);
    }

    public class OrderReturnCommand : IOrderReturnCommand
    {
        private readonly IOrderCloudClient oc;
        private readonly IHSCreditCardProcessor creditCardService;
        private readonly IOrderReturnIntegrationEventCommand orderReturnIntegrationEventCommand;

        public OrderReturnCommand(IOrderCloudClient oc, IHSCreditCardProcessor creditCardService, IOrderReturnIntegrationEventCommand orderReturnIntegrationEventCommand)
        {
            this.oc = oc;
            this.creditCardService = creditCardService;
            this.orderReturnIntegrationEventCommand = orderReturnIntegrationEventCommand;
        }

        public async Task<HSOrderReturn> CompleteReturn(string orderReturnId)
        {
            var orderReturn = await oc.OrderReturns.GetAsync<HSOrderReturn>(orderReturnId);
            Require.That(orderReturn.Status == OrderStatus.Open, new Exception("Order Return must be approved in order to complete"));

            // get payment to refund, there should only be one payment on the order in headstart
            var paymentList = await oc.Payments.ListAsync<HSPayment>(OrderDirection.All, orderReturn.OrderID);
            var payment = paymentList.Items.FirstOrDefault();

            if (payment.Type == PaymentType.CreditCard)
            {
                var creditCardPaymentTransaction = payment.Transactions
                    .OrderBy(x => x.DateExecuted)
                    .LastOrDefault(x => x.Type == "CreditCard" && x.Succeeded);

                // make inquiry to determine the current capture capture state
                var order = await oc.Orders.GetAsync<HSOrder>(OrderDirection.All, orderReturn.OrderID);
                var inquiryResult = await creditCardService.Inquire(order, creditCardPaymentTransaction);

                // Transactions that are queued for capture can only be fully voided, and we are only allowing partial voids moving forward.
                if (inquiryResult.PendingCapture)
                {
                    throw new CatalystBaseException(new ApiError
                    {
                        ErrorCode = "Payment.FailedToVoidAuthorization",
                        Message = "This customer's credit card transaction is currently queued for capture and cannot be refunded at this time.  Please try again later.",
                    });
                }

                // If voidable, but not refundable, void the refund amount off the original order total
                if (inquiryResult.CanVoid)
                {
                    await creditCardService.VoidAuthorization(order, payment, creditCardPaymentTransaction, orderReturn.RefundAmount, orderReturnId);
                }

                // If refundable, but not voidable, do a refund
                if (inquiryResult.CanRefund)
                {
                    await creditCardService.Refund(order, payment, creditCardPaymentTransaction, (decimal)orderReturn.RefundAmount, orderReturnId);
                }
            }
            else
            {
                // Create a payment with a negative amount equal to refund amount since we are issuing a credit
                // this will automatically transition the order return status to complete
                // creditCardService.Refund will automatically create this payment so only necessary to create on on purchase order and spending account type
                await oc.Payments.CreateAsync(OrderDirection.All, orderReturn.OrderID, new HSPayment
                {
                    Type = payment.Type,
                    SpendingAccountID = payment.SpendingAccountID,
                    Amount = orderReturn.RefundAmount * -1, // make this amount negative since we are issuing a credit
                    OrderReturnID = orderReturnId,
                    xp = payment.xp,
                });
            }

            return await oc.OrderReturns.GetAsync<HSOrderReturn>(orderReturnId);
        }

        public async Task<IEnumerable<LineItemReturnCalculation>> CalculateOrderReturn(string orderId, List<OrderReturnItem> itemsToReturn)
        {
            var worksheet = await oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.All, orderId);

            // build a fake order return just so we can get calculations
            var orderReturn = new HSOrderReturn
            {
                ItemsToReturn = itemsToReturn,
            };
            var result = await orderReturnIntegrationEventCommand.CalculateOrderReturn(worksheet, orderReturn);
            return result.ItemsToReturnCalcs;
        }
    }
}
