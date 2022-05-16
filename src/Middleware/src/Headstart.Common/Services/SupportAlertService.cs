using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.Models;
using Headstart.Models.Headstart;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using OrderCloud.Integrations.CardConnect;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.Common.Services
{
    public interface ISupportAlertService
    {
        Task VoidAuthorizationFailed(HSPayment payment, string transactionID, HSOrder order, CreditCardVoidException ex);
    }

    // use this service to alert support of critical failures
    public class SupportAlertService : ISupportAlertService
    {
        private readonly TelemetryClient telemetry;
        private readonly ISendgridService sendgrid;
        private readonly AppSettings settings;

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
                this.settings = settings;
                this.telemetry = telemetry;
                this.sendgrid = sendgrid;
            }
            catch (Exception ex)
            {
                LoggingNotifications.LogApiResponseMessages(this.settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
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
            await sendgrid.EmailVoidAuthorizationFailedAsync(payment, transactionID, order, ex);
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
                { "ErrorResponse", JsonConvert.SerializeObject(ex.ApiError, Formatting.Indented) },
            };
            telemetry.TrackEvent("Payment.VoidAuthorizationFailed", customProperties);
        }
    }
}
