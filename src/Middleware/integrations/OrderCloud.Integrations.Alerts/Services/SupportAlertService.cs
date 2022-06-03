using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using OrderCloud.Integrations.Emails;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Alerts
{
    public interface ISupportAlertService
    {
        Task VoidAuthorizationFailed(HSPayment payment, string transactionID, HSOrder order, ApiError apiError);
    }

    // use this service to alert support of critical failures
    public class SupportAlertService : ISupportAlertService
    {
        private readonly TelemetryClient telemetry;
        private readonly IEmailServiceProvider emailServiceProvider;

        public SupportAlertService(TelemetryClient telemetry, IEmailServiceProvider emailServiceProvider)
        {
            this.telemetry = telemetry;
            this.emailServiceProvider = emailServiceProvider;
        }

        public async Task VoidAuthorizationFailed(HSPayment payment, string transactionID, HSOrder order, ApiError apiError)
        {
            LogVoidAuthorizationFailed(payment, transactionID, order, apiError);
            await emailServiceProvider.EmailVoidAuthorizationFailedAsync(payment, transactionID, order, apiError);
        }

        public void LogVoidAuthorizationFailed(HSPayment payment, string transactionID, HSOrder order, ApiError apiError)
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
                    { "ErrorResponse", JsonConvert.SerializeObject(apiError, Formatting.Indented) },
                };
            telemetry.TrackEvent("Payment.VoidAuthorizationFailed", customProperties);
        }
    }
}
