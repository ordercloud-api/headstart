using Headstart.Models;
using Microsoft.ApplicationInsights;
using Newtonsoft.Json;
using ordercloud.integrations.cardconnect;
using SendGrid.Helpers.Mail;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Headstart.Models.Headstart;

namespace Headstart.Common.Services
{
    public interface ISupportAlertService
    {
        Task VoidAuthorizationFailed(HSPayment payment, string transactionID, HSOrder order, CreditCardVoidException ex);
    }

    // use this service to alert support of critical failures
    public class SupportAlertService : ISupportAlertService
    {
        private readonly TelemetryClient _telemetry;
        private readonly ISendgridService _sendgrid;
        private readonly AppSettings _settings;
        public SupportAlertService(TelemetryClient telemetry, ISendgridService sendgrid, AppSettings settings)
        {
            _telemetry = telemetry;
            _sendgrid = sendgrid;
            _settings = settings;
        }

        public async Task VoidAuthorizationFailed(HSPayment payment, string transactionID, HSOrder order, CreditCardVoidException ex)
        {
            LogVoidAuthorizationFailed(payment, transactionID, order, ex);
            await _sendgrid.EmailVoidAuthorizationFailedAsync(payment, transactionID, order, ex);
        }

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
