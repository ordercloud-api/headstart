using Headstart.Common;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using ordercloud.integrations.cardconnect;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using Sitecore.Diagnostics;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

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

		/// <summary>
		/// The IOC based constructor method for the OrderSubmitCommand class object with Dependency Injection
		/// </summary>
		/// <param name="oc"></param>
		/// <param name="settings"></param>
		/// <param name="card"></param>
		public OrderSubmitCommand(IOrderCloudClient oc, AppSettings settings, ICreditCardCommand card)
		{
			try
			{
				_oc = oc;
				_settings = settings;
				_card = card;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable SubmitOrderAsync task method
		/// </summary>
		/// <param name="orderID"></param>
		/// <param name="direction"></param>
		/// <param name="payment"></param>
		/// <param name="userToken"></param>
		/// <returns>The HSOrder object from the SubmitOrderAsync process</returns>
		public async Task<HSOrder> SubmitOrderAsync(string orderID, OrderDirection direction, OrderCloudIntegrationsCreditCardPayment payment, string userToken)
		{
			var worksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HSOrderWorksheet>(OrderDirection.Incoming, orderID);
			await ValidateOrderAsync(worksheet, payment, userToken);

			var incrementedOrderID = await IncrementOrderAsync(worksheet);
			// If Credit Card info is null, payment is a Purchase Order, thus skip CC validation
			if (payment.CreditCardDetails != null || payment.CreditCardID != null)
			{
				payment.OrderID = incrementedOrderID;
				await _card.AuthorizePayment(payment, userToken, GetMerchantID(payment));
			}
			try
			{
				return await _oc.Orders.SubmitAsync<HSOrder>(direction, incrementedOrderID, userToken);
			}
			catch (Exception)
			{
				await _card.VoidPaymentAsync(incrementedOrderID, userToken);
				throw;
			}
		}

		/// <summary>
		/// Private re-usable ValidateOrderAsync task method
		/// </summary>
		/// <param name="worksheet"></param>
		/// <param name="payment"></param>
		/// <param name="userToken"></param>
		/// <returns></returns>
		/// <exception cref="CatalystBaseException"></exception>
		private async Task ValidateOrderAsync(HSOrderWorksheet worksheet, OrderCloudIntegrationsCreditCardPayment payment, string userToken)
		{
			Require.That(
				!worksheet.Order.IsSubmitted, 
				new ErrorCode("OrderSubmit.AlreadySubmitted", "Order has already been submitted")
			);

			var shipMethodsWithoutSelections = worksheet?.ShipEstimateResponse?.ShipEstimates?.Where(estimate => estimate.SelectedShipMethodID == null);
			Require.That(
				worksheet?.ShipEstimateResponse != null &&
				shipMethodsWithoutSelections.Count() == 0, 
				new ErrorCode("OrderSubmit.MissingShippingSelections", "All shipments on an order must have a selection"), shipMethodsWithoutSelections
			);

			Require.That(
				!worksheet.LineItems.Any() || payment != null,
				new ErrorCode("OrderSubmit.MissingPayment", "Order contains standard line items and must include credit card payment details"),
				worksheet.LineItems
			);
			var lineItemsInactive = await GetInactiveLineItems(worksheet, userToken);
			Require.That(
				!lineItemsInactive.Any(),
				new ErrorCode("OrderSubmit.InvalidProducts", "Order contains line items for products that are inactive"), lineItemsInactive
			);

			try
			{
				// ordercloud validates the same stuff that would be checked on order submit
				await _oc.Orders.ValidateAsync(OrderDirection.Incoming, worksheet.Order.ID);
			} catch(OrderCloudException ex) {
				// credit card payments aren't accepted yet, so ignore this error for now
				// we'll accept the payment once the credit card auth goes through (before order submit)
				var errors = ex.Errors.Where(ex => ex.ErrorCode != "Order.CannotSubmitWithUnaccceptedPayments");
				if(errors.Any())
				{
					throw new CatalystBaseException("OrderSubmit.OrderCloudValidationError", "Failed ordercloud validation, see Data for details", errors);
				}
			}
		}

		/// <summary>
		/// Private re-usable GetInactiveLineItems task method
		/// </summary>
		/// <param name="worksheet"></param>
		/// <param name="userToken"></param>
		/// <returns>The list of HSLineItem object from the GetInactiveLineItems process</returns>
		private async Task<List<HSLineItem>> GetInactiveLineItems(HSOrderWorksheet worksheet, string userToken)
		{
			List<HSLineItem> inactiveLineItems = new List<HSLineItem>();
			foreach (HSLineItem lineItem in worksheet.LineItems)
			{
				try
				{
					await _oc.Me.GetProductAsync(lineItem.ProductID, sellerID: _settings.OrderCloudSettings.MarketplaceID, accessToken: userToken);
				}
				catch (OrderCloudException ex) when (ex.HttpStatus == HttpStatusCode.NotFound)
				{
					inactiveLineItems.Add(lineItem);
				}
			}
			return inactiveLineItems;
		}

		/// <summary>
		/// Private re-usable IncrementOrderAsync task method
		/// </summary>
		/// <param name="worksheet"></param>
		/// <returns>The order.ID string value from the IncrementOrderAsync process</returns>
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

		/// <summary>
		/// Private re-usable GetMerchantID task method
		/// </summary>
		/// <param name="payment"></param>
		/// <returns>The merchantID string value from the GetMerchantID process</returns>
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
	}
}