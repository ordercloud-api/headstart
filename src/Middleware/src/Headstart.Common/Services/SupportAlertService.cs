using System;
using Newtonsoft.Json;
using System.Threading.Tasks;
using System.Collections.Generic;
using Microsoft.ApplicationInsights;
using Headstart.Models;
using Headstart.Models.Headstart;
using ordercloud.integrations.cardconnect;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.Common.Services
{
	public interface ISupportAlertService
	{
		Task VoidAuthorizationFailed(HSPayment payment, string transactionID, HSOrder order, CreditCardVoidException ex);
	}

	public class SupportAlertService : ISupportAlertService
	{
		private readonly TelemetryClient _telemetry;
		private readonly ISendgridService _sendgrid;
		private readonly AppSettings _settings;

		/// <summary>
		/// The IOC based constructor method for the SupportAlertService class object with Dependency Injection
		/// </summary>
		/// <param name="telemetry"></param>
		/// <param name="sendgrid"></param>
		/// <param name="settings"></param>
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
				LoggingNotifications.LogApiResponseMessages(_settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
		}

		/// <summary>
		/// Public re-usable VoidAuthorizationFailed task method
		/// </summary>
		/// <param name="payment"></param>
		/// <param name="transactionID"></param>
		/// <param name="order"></param>
		/// <param name="ex"></param>
		/// <returns></returns>
		public async Task VoidAuthorizationFailed(HSPayment payment, string transactionID, HSOrder order, CreditCardVoidException ex)
		{
			LogVoidAuthorizationFailed(payment, transactionID, order, ex);
			await _sendgrid.EmailVoidAuthorizationFailedAsync(payment, transactionID, order, ex);
		}

		/// <summary>
		/// Public re-usable LogVoidAuthorizationFailed method
		/// </summary>
		/// <param name="payment"></param>
		/// <param name="transactionID"></param>
		/// <param name="order"></param>
		/// <param name="ex"></param>
		public void LogVoidAuthorizationFailed(HSPayment payment, string transactionID, HSOrder order, CreditCardVoidException ex)
		{
			// track in app insights
			// to find go to Transaction Search > Event Type = Event > Filter by any of these custom properties or event name "Payment.VoidAuthorizationFailed"
			var customProperties = new Dictionary<string, string>
			{
				{ "Message", "Attempt to void authorization on payment failed" },
				{ "OrderID", order.ID },
				{ "BuyerID", order.FromCompanyID },
				{ "UserEmail", order.FromUser.Email },
				{ "PaymentID", payment.ID },
				{ "TransactionID", transactionID },
				{ "ErrorResponse", JsonConvert.SerializeObject(ex.ApiError, Formatting.Indented)}
			};
			_telemetry.TrackEvent("Payment.VoidAuthorizationFailed", customProperties);
		}
	}
}