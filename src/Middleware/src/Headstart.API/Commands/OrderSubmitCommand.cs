using System;
using System.Net;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Common;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Headstart.Common.Models.Headstart;
using ordercloud.integrations.cardconnect;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extensions;

namespace Headstart.API.Commands
{
	public interface IOrderSubmitCommand
	{
		Task<HsOrder> SubmitOrderAsync(string orderID, OrderDirection direction, OrderCloudIntegrationsCreditCardPayment payment, string userToken);
	}
    
	public class OrderSubmitCommand : IOrderSubmitCommand
	{
		private readonly IOrderCloudClient _oc;
		private readonly AppSettings _settings;
		private readonly ICreditCardCommand _card;
		private readonly ConfigSettings _configSettings = ConfigSettings.Instance;

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
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable SubmitOrderAsync task method
		/// </summary>
		/// <param name="orderId"></param>
		/// <param name="direction"></param>
		/// <param name="payment"></param>
		/// <param name="userToken"></param>
		/// <returns>The HsOrder response object from the SubmitOrderAsync process</returns>
		public async Task<HsOrder> SubmitOrderAsync(string orderId, OrderDirection direction, OrderCloudIntegrationsCreditCardPayment payment, string userToken)
		{
			var resp = new HsOrder();
			var incrementedOrderId = string.Empty;
			try
			{
				var worksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, orderId);
				await ValidateOrderAsync(worksheet, payment, userToken);

				incrementedOrderId = await IncrementOrderAsync(worksheet);
				// If Credit Card info is null, payment is a Purchase Order, thus skip CC validation
				if (payment.CreditCardDetails != null || payment.CreditCardID != null)
				{
					payment.OrderID = incrementedOrderId;
					await _card.AuthorizePayment(payment, userToken, GetMerchantId(payment));
				}
				resp = await _oc.Orders.SubmitAsync<HsOrder>(direction, incrementedOrderId, userToken);
			}
			catch (Exception ex)
			{
				await _card.VoidPaymentAsync(incrementedOrderId, userToken);
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
				throw ex;
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable ValidateOrderAsync task method
		/// </summary>
		/// <param name="worksheet"></param>
		/// <param name="payment"></param>
		/// <param name="userToken"></param>
		/// <returns></returns>
		/// <exception cref="CatalystBaseException"></exception>
		private async Task ValidateOrderAsync(HsOrderWorksheet worksheet, OrderCloudIntegrationsCreditCardPayment payment, string userToken)
		{
			try
			{
				Require.That(!worksheet.Order.IsSubmitted, new ErrorCode(@"OrderSubmit.AlreadySubmitted", @"This Order has already been submitted."));
				var shipMethodsWithoutSelections = worksheet?.ShipEstimateResponse?.ShipEstimates?.Where(estimate => estimate.SelectedShipMethodID == null);
				Require.That(worksheet?.ShipEstimateResponse != null && shipMethodsWithoutSelections.Count() == 0, new ErrorCode(@"OrderSubmit.MissingShippingSelections", @"All shipments on an order must have a selection."), shipMethodsWithoutSelections);

				Require.That(!worksheet.LineItems.Any() || payment != null, new ErrorCode(@"OrderSubmit.MissingPayment", @"This Order contains standard line items and must include credit card payment details."), worksheet.LineItems);
				var lineItemsInactive = await GetInactiveLineItems(worksheet, userToken);
				Require.That(!lineItemsInactive.Any(), new ErrorCode(@"OrderSubmit.InvalidProducts", @"This Order contains line items for products that are inactive."), lineItemsInactive);

				// Ordercloud validates the same stuff that would be checked on order submit
				await _oc.Orders.ValidateAsync(OrderDirection.Incoming, worksheet.Order.ID);
			} 
			catch (OrderCloudException ex) {
				// Credit card payments aren't accepted yet, so ignore this error for now
				// we'll accept the payment once the credit card auth goes through (before order submit)
				var errors = ex.Errors.Where(ex => (!ex.ErrorCode.Equals(@"Order.CannotSubmitWithUnacceptedPayments", StringComparison.OrdinalIgnoreCase)));
				if(errors.Any())
				{
					var ex1 = new CatalystBaseException(@"OrderSubmit.OrderCloudValidationError", @"This is a failed ordercloud validation, see Data for details.", errors);
					LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", $@"{ex.Message}. {ex1.Message}", ex1.StackTrace, this, true);
				}
			}
		}

		/// <summary>
		/// Private re-usable GetInactiveLineItems task method
		/// </summary>
		/// <param name="worksheet"></param>
		/// <param name="userToken"></param>
		/// <returns>The list of HsLineItem response object from the GetInactiveLineItems process</returns>
		private async Task<List<HsLineItem>> GetInactiveLineItems(HsOrderWorksheet worksheet, string userToken)
		{
			var inactiveLineItems = new List<HsLineItem>();
			try
			{
				foreach (var lineItem in worksheet.LineItems)
				{
					try
					{
						await _oc.Me.GetProductAsync(lineItem.ProductID, sellerID: _settings.OrderCloudSettings.MarketplaceId, accessToken: userToken);
					}
					catch (OrderCloudException ex) when (ex.HttpStatus == HttpStatusCode.NotFound)
					{
						inactiveLineItems.Add(lineItem);
						LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
					}
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return inactiveLineItems;
		}

		/// <summary>
		/// Private re-usable IncrementOrderAsync task method
		/// </summary>
		/// <param name="worksheet"></param>
		/// <returns>The order.ID string value from the IncrementOrderAsync process</returns>
		private async Task<string> IncrementOrderAsync(HsOrderWorksheet worksheet)
		{
			var order = new Order();
			try
			{
				if (worksheet.Order.xp.IsResubmitting == true)
				{
					// orders marked with IsResubmitting true are orders that were on hold and then declined 
					// so buyer needs to resubmit but we don't want to increment order again
					return worksheet.Order.ID;
				}
				if (worksheet.Order.ID.StartsWith(_settings.OrderCloudSettings.IncrementorPrefix))
				{
					// order has already been incremented, no need to increment again
					return worksheet.Order.ID;
				}
				order = await _oc.Orders.PatchAsync(OrderDirection.Incoming, worksheet.Order.ID, new PartialOrder
				{
					ID = (_settings.OrderCloudSettings.IncrementorPrefix + "{orderIncrementor}")
				});
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return order.ID;
		}

		/// <summary>
		/// Private re-usable GetMerchantID task method
		/// </summary>
		/// <param name="payment"></param>
		/// <returns>The merchantID string value from the GetMerchantID process</returns>
		private string GetMerchantId(OrderCloudIntegrationsCreditCardPayment payment)
		{
			var merchantId = string.Empty;
			try
			{
				if (payment.Currency.Equals(@"USD", StringComparison.OrdinalIgnoreCase))
				{
					merchantId = _settings.CardConnectSettings.UsdMerchantID;
				}
				else if (payment.Currency.Equals(@"CAD", StringComparison.OrdinalIgnoreCase))
				{
					merchantId = _settings.CardConnectSettings.CadMerchantID;
				}
				else
				{
					merchantId = _settings.CardConnectSettings.EurMerchantID;
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_configSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return merchantId;
		}
	}
}