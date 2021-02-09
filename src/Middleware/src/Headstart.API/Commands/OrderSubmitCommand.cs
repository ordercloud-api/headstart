using Headstart.Models;
using ordercloud.integrations.cardconnect;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using Polly;
using Polly.Retry;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.ApplicationInsights;
using Microsoft.ApplicationInsights.Extensibility;
using System.Net;
using Newtonsoft.Json;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models.Headstart;
using Headstart.Common;

namespace Headstart.API.Commands
{
    public interface IOrderSubmitCommand
    {
        Task<HSOrder> SubmitOrderAsync(string orderID, OrderDirection direction, OrderCloudIntegrationsCreditCardPayment payment, string userToken);
    }
    public class OrderSubmitCommand : IOrderSubmitCommand
    {
        private readonly IOrderCloudClient _oc;
        private readonly AppSettings _settings;
        private readonly ICreditCardCommand _card;

        public OrderSubmitCommand(IOrderCloudClient oc, AppSettings settings, ICreditCardCommand card)
        {
            _oc = oc;
            _settings = settings;
            _card = card;
        }

        public async Task<HSOrder> SubmitOrderAsync(string orderID, OrderDirection direction, OrderCloudIntegrationsCreditCardPayment payment, string userToken)
        {
            var worksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID);
            await ValidateOrderAsync(worksheet, payment, userToken);

            var incrementedOrderID = await IncrementOrderAsync(worksheet);
            if (worksheet.LineItems.Any(li => li.Product.xp.ProductType != ProductType.PurchaseOrder))
            {
                payment.OrderID = incrementedOrderID;
                await _card.AuthorizePayment(payment, userToken, GetMerchantID(payment));
            }
            try
            {
                return await WithRetry().ExecuteAsync(() => _oc.Orders.SubmitAsync<HSOrder>(direction, incrementedOrderID, userToken));
            }
            catch (Exception)
            {
                await _card.VoidPaymentAsync(incrementedOrderID, userToken);
                throw;
            }
        }

        private async Task ValidateOrderAsync(HSOrderWorksheet worksheet, OrderCloudIntegrationsCreditCardPayment payment, string userToken)
        {
            Require.That(
                !worksheet.Order.IsSubmitted, 
                new ErrorCode("OrderSubmit.AlreadySubmitted", 400, "Order has already been submitted")
            );

            var shipMethodsWithoutSelections = worksheet.ShipEstimateResponse.ShipEstimates.Where(estimate => estimate.SelectedShipMethodID == null);
            Require.That(
                shipMethodsWithoutSelections.Count() == 0, 
                new ErrorCode("OrderSubmit.MissingShippingSelections", 400, "All shipments on an order must have a selection"), shipMethodsWithoutSelections
                );

            var standardLines = worksheet.LineItems.Where(li => li.Product.xp.ProductType != ProductType.PurchaseOrder);
            Require.That(
                !standardLines.Any() || payment != null,
                new ErrorCode("OrderSubmit.MissingPayment", 400, "Order contains standard line items and must include credit card payment details"),
                standardLines
            );
            var lineItemsInactive = await GetInactiveLineItems(worksheet, userToken);
            Require.That(
                !lineItemsInactive.Any(),
                new ErrorCode("OrderSubmit.InvalidProducts", 400, "Order contains line items for products that are inactive"), lineItemsInactive
            );

            try
            {
                // ordercloud validates the same stuff that would be checked on order submit
                await _oc.Orders.ValidateAsync(OrderDirection.Incoming, worksheet.Order.ID);
            } catch(OrderCloudException ex) {
                // this error is expected and will be resolved before oc order submit call happens
                // in a non-seb flow this could be removed because we'd auth the payment which would mark it as accepted
                // before it even hits the submit endpoint
                var errors = ex.Errors.Where(ex => ex.ErrorCode != "Order.CannotSubmitWithUnaccceptedPayments");
                if(errors.Any())
                {
                    throw new OrderCloudIntegrationException(new ApiError
                    {
                        ErrorCode = "OrderSubmit.OrderCloudValidationError",
                        Message = "Failed ordercloud validation, see Data for details",
                        Data = errors
                    });
                }
            }
            
        }

        private async Task<List<HSLineItem>> GetInactiveLineItems(HSOrderWorksheet worksheet, string userToken)
        {
            List<HSLineItem> inactiveLineItems = new List<HSLineItem>();
            foreach (HSLineItem lineItem in worksheet.LineItems)
            {
                try
                {
                    await _oc.Me.GetProductAsync(lineItem.ProductID, accessToken: userToken);
                }
                catch (OrderCloudException ex) when (ex.HttpStatus == HttpStatusCode.NotFound)
                {
                    inactiveLineItems.Add(lineItem);
                }
            }
            return inactiveLineItems;
        }

        private async Task<string> IncrementOrderAsync(HSOrderWorksheet worksheet)
        {
            if (worksheet.Order.xp.IsResubmitting == true)
            {
                // orders marked with IsResubmitting true are orders that were on hold and then declined 
                // so buyer needs to resubmit but we don't want to increment order again
                return worksheet.Order.ID;
            }
            if(worksheet.Order.ID.StartsWith(_settings.OrderCloudSettings.IncrementorPrefix))
            {
                // order has already been incremented, no need to increment again
                return worksheet.Order.ID;
            }
            var order = await _oc.Orders.PatchAsync(OrderDirection.Incoming, worksheet.Order.ID, new PartialOrder
            {
                ID = _settings.OrderCloudSettings.IncrementorPrefix + "{orderIncrementor}"
            });
            return order.ID;
        }

        private string GetMerchantID(OrderCloudIntegrationsCreditCardPayment payment)
        {
            string merchantID;
            if (payment.Currency == "USD")
                merchantID = _settings.CardConnectSettings.UsdMerchantID;
            else if (payment.Currency == "CAD")
                merchantID = _settings.CardConnectSettings.CadMerchantID;
            else
                merchantID = _settings.CardConnectSettings.EurMerchantID;

            return merchantID;
        }

        private AsyncRetryPolicy WithRetry()
        {
            // retries three times, waits two seconds in-between failures
            return Policy
                .Handle<OrderCloudException>(e => e.HttpStatus == HttpStatusCode.InternalServerError || e.HttpStatus == HttpStatusCode.RequestTimeout)
                .WaitAndRetryAsync(new[] {
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(2),
                    TimeSpan.FromSeconds(2),
                });
        }
    }
}
