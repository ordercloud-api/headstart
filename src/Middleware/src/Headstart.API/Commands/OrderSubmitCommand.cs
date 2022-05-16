using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Headstart.Common;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.CardConnect.Models;
using OrderCloud.SDK;

namespace Headstart.API.Commands
{
    public interface IOrderSubmitCommand
    {
        Task<HSOrder> SubmitOrderAsync(string orderID, OrderDirection direction, OrderCloudIntegrationsCreditCardPayment payment, string userToken);
    }

    public class OrderSubmitCommand : IOrderSubmitCommand
    {
        private readonly IOrderCloudClient oc;
        private readonly AppSettings settings;
        private readonly ICreditCardCommand card;

        public OrderSubmitCommand(IOrderCloudClient oc, AppSettings settings, ICreditCardCommand card)
        {
            this.oc = oc;
            this.settings = settings;
            this.card = card;
        }

        public async Task<HSOrder> SubmitOrderAsync(string orderID, OrderDirection direction, OrderCloudIntegrationsCreditCardPayment payment, string userToken)
        {
            var worksheet = await oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID);
            await ValidateOrderAsync(worksheet, payment, userToken);

            var incrementedOrderID = await IncrementOrderAsync(worksheet);

            // If Credit Card info is null, payment is a Purchase Order, thus skip CC validation
            if (payment.CreditCardDetails != null || payment.CreditCardID != null)
            {
                payment.OrderID = incrementedOrderID;
                await card.AuthorizePayment(payment, userToken, GetMerchantID(payment));
            }

            try
            {
                return await oc.Orders.SubmitAsync<HSOrder>(direction, incrementedOrderID, userToken);
            }
            catch (Exception)
            {
                await card.VoidPaymentAsync(incrementedOrderID, userToken);
                throw;
            }
        }

        private async Task ValidateOrderAsync(HSOrderWorksheet worksheet, OrderCloudIntegrationsCreditCardPayment payment, string userToken)
        {
            Require.That(
                !worksheet.Order.IsSubmitted,
                new ErrorCode("OrderSubmit.AlreadySubmitted", "Order has already been submitted"));

            var shipMethodsWithoutSelections = worksheet?.ShipEstimateResponse?.ShipEstimates?.Where(estimate => estimate.SelectedShipMethodID == null);
            Require.That(
                worksheet?.ShipEstimateResponse != null &&
                shipMethodsWithoutSelections.Count() == 0,
                new ErrorCode("OrderSubmit.MissingShippingSelections", "All shipments on an order must have a selection"),
                shipMethodsWithoutSelections);

            Require.That(
                !worksheet.LineItems.Any() || payment != null,
                new ErrorCode("OrderSubmit.MissingPayment", "Order contains standard line items and must include credit card payment details"),
                worksheet.LineItems);
            var lineItemsInactive = await GetInactiveLineItems(worksheet, userToken);
            Require.That(
                !lineItemsInactive.Any(),
                new ErrorCode("OrderSubmit.InvalidProducts", "Order contains line items for products that are inactive"),
                lineItemsInactive);

            try
            {
                // ordercloud validates the same stuff that would be checked on order submit
                await oc.Orders.ValidateAsync(OrderDirection.Incoming, worksheet.Order.ID);
            }
            catch (OrderCloudException ex)
            {
                // credit card payments aren't accepted yet, so ignore this error for now
                // we'll accept the payment once the credit card auth goes through (before order submit)
                var errors = ex.Errors.Where(ex => ex.ErrorCode != "Order.CannotSubmitWithUnaccceptedPayments");
                if (errors.Any())
                {
                    throw new CatalystBaseException("OrderSubmit.OrderCloudValidationError", "Failed ordercloud validation, see Data for details", errors);
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
                    await oc.Me.GetProductAsync(lineItem.ProductID, sellerID: settings.OrderCloudSettings.MarketplaceID, accessToken: userToken);
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

            if (worksheet.Order.ID.StartsWith(settings.OrderCloudSettings.IncrementorPrefix))
            {
                // order has already been incremented, no need to increment again
                return worksheet.Order.ID;
            }

            var order = await oc.Orders.PatchAsync(OrderDirection.Incoming, worksheet.Order.ID, new PartialOrder
            {
                ID = settings.OrderCloudSettings.IncrementorPrefix + "{orderIncrementor}",
            });
            return order.ID;
        }

        private string GetMerchantID(OrderCloudIntegrationsCreditCardPayment payment)
        {
            string merchantID;
            if (payment.Currency == "USD")
            {
                merchantID = settings.CardConnectSettings.UsdMerchantID;
            }
            else if (payment.Currency == "CAD")
            {
                merchantID = settings.CardConnectSettings.CadMerchantID;
            }
            else
            {
                merchantID = settings.CardConnectSettings.EurMerchantID;
            }

            return merchantID;
        }
    }
}
