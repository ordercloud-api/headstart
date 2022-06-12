using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Emails
{
    public class DefaultEmailServiceProvider : IEmailServiceProvider
    {
        public DefaultEmailServiceProvider()
        {
        }

        public Task EmailGeneralSupportQueue(SupportCase supportCase)
        {
            return Task.FromResult<object>(null);
        }

        public Task EmailVoidAuthorizationFailedAsync(HSPayment payment, string transactionID, HSOrder order, ApiError apiError)
        {
            return Task.FromResult<object>(null);
        }

        public Task SendContactSupplierAboutProductEmail(ContactSupplierBody template)
        {
            return Task.FromResult<object>(null);
        }

        public Task SendLineItemStatusChangeEmail(HSOrder order, LineItemStatusChanges lineItemStatusChanges, List<HSLineItem> lineItems, string firstName, string lastName, string email, EmailDisplayText lineItemEmailDisplayText)
        {
            return Task.FromResult<object>(null);
        }

        public Task SendLineItemStatusChangeEmailMultipleRcpts(HSOrder order, LineItemStatusChanges lineItemStatusChanges, List<HSLineItem> lineItems, List<string> tos, EmailDisplayText lineItemEmailDisplayText)
        {
            return Task.FromResult<object>(null);
        }

        public Task SendNewUserEmail(MessageNotification<PasswordResetEventBody> payload)
        {
            return Task.FromResult<object>(null);
        }

        public Task SendOrderApprovedEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            return Task.FromResult<object>(null);
        }

        public Task SendOrderDeclinedEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            return Task.FromResult<object>(null);
        }

        public Task SendOrderRequiresApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            return Task.FromResult<object>(null);
        }

        public Task SendOrderSubmitEmail(HSOrderWorksheet orderData)
        {
            return Task.FromResult<object>(null);
        }

        public Task SendOrderSubmittedForApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification)
        {
            return Task.FromResult<object>(null);
        }

        public Task SendPasswordResetEmail(MessageNotification<PasswordResetEventBody> messageNotification)
        {
            return Task.FromResult<object>(null);
        }

        public Task SendQuotePriceConfirmationEmail(HSOrder order, HSLineItem lineItem, string buyerEmail)
        {
            return Task.FromResult<object>(null);
        }

        public Task SendQuoteRequestConfirmationEmail(HSOrder order, HSLineItem lineItem, string buyerEmail)
        {
            return Task.FromResult<object>(null);
        }

        public Task SendSingleTemplateEmail(string from, string to, string templateID, object templateData)
        {
            return Task.FromResult<object>(null);
        }

        public Task SendSingleTemplateEmailMultipleRcpts(string from, List<string> tos, string templateID, object templateData)
        {
            return Task.FromResult<object>(null);
        }

        public Task SendSingleTemplateEmailMultipleRcptsAttachment(string from, List<string> tos, string templateID, object templateData, CloudAppendBlob fileReference, string fileName)
        {
            return Task.FromResult<object>(null);
        }

        public Task SendSingleTemplateEmailSingleRcptAttachment(string from, string to, string templateID, object templateData, IFormFile fileReference)
        {
            return Task.FromResult<object>(null);
        }
    }
}
