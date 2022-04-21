using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Headstart.Common.Models.Headstart;
using ordercloud.integrations.cardconnect;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.Common.Services
{
	public interface ISupportAlertService
	{
		Task VoidAuthorizationFailed(HsPayment payment, string transactionID, HsOrder order, CreditCardVoidException ex);
	}

	public class SupportAlertService : ISupportAlertService
	{
		private readonly TelemetryClient _telemetry;
		private readonly ISendgridService _sendgrid;
		private readonly AppSettings _settings;

		public SupportAlertService(TelemetryClient telemetry, ISendgridService sendgrid, AppSettings settings)
		{
			try
			{
				_telemetry = telemetry;
				_sendgrid = sendgrid;
				_settings = settings;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(_settings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
		}

		public async Task VoidAuthorizationFailed(HsPayment payment, string transactionId, HsOrder order, CreditCardVoidException ex)
		{
			try
			{
				LogVoidAuthorizationFailed(payment, transactionId, order, ex);
				await _sendgrid.EmailVoidAuthorizationFailedAsync(payment, transactionId, order, ex);
			}
			catch (Exception ex1)
			{
				var responseBody = ex != null ? JsonConvert.SerializeObject(ex) : string.Empty;
				LoggingNotifications.LogApiResponseMessages(_settings, SitecoreExtensions.Helpers.GetMethodName(), responseBody,
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex1.Message, ex1.StackTrace, ex1);
			}
		}

		public void LogVoidAuthorizationFailed(HsPayment payment, string transactionId, HsOrder order, CreditCardVoidException ex)
		{
			try
			{
				// Track in app insights to find go to Transaction Search > Event Type = Event > Filter by any
				// of these custom properties or event name "Payment.VoidAuthorizationFailed"
				var customProperties = new Dictionary<string, string>
				{
					{
						@"Message", @"Attempt to void authorization on payment failed."
					},
					{
						@"OrderID", order.ID
					},
					{
						@"BuyerID", order.FromCompanyID
					},
					{
						@"UserEmail", order.FromUser.Email
					},
					{
						@"PaymentID", payment.ID
					},
					{
						@"TransactionID", transactionId
					},
					{
						@"ErrorResponse", JsonConvert.SerializeObject(ex.ApiError, Formatting.Indented)
					}
				};
				_telemetry.TrackEvent(@"Payment.VoidAuthorizationFailed", customProperties);
			}
			catch (Exception ex1)
			{
				var responseBody = ex != null ? JsonConvert.SerializeObject(ex) : string.Empty;
				LoggingNotifications.LogApiResponseMessages(_settings, SitecoreExtensions.Helpers.GetMethodName(), responseBody,
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex1.Message, ex1.StackTrace, ex1);
			}
		}
	}
}