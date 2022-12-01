using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Emails
{
    public interface IEmailServiceProvider
    {
        Task SendSingleTemplateEmail(string from, string to, string templateID, object templateData);

        Task SendSingleTemplateEmailMultipleRcpts(string from, List<string> tos, string templateID, object templateData);

        Task SendSingleTemplateEmailMultipleRcptsAttachment(string from, List<string> tos, string templateID, object templateData, CloudAppendBlob fileReference, string fileName);

        Task SendSingleTemplateEmailSingleRcptAttachment(string from, string to, string templateID, object templateData, IFormFile fileReference);

        Task SendOrderSubmitEmail(HSOrderWorksheet orderData);

        Task SendNewUserEmail(MessageNotification<PasswordResetEventBody> payload);

        Task SendPasswordResetEmail(MessageNotification<PasswordResetEventBody> messageNotification);

        Task SendOrderRequiresApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification);

        Task SendOrderSubmittedForApprovalEmail(MessageNotification<OrderSubmitEventBody> messageNotification);

        Task SendOrderApprovedEmail(MessageNotification<OrderSubmitEventBody> messageNotification);

        Task SendOrderDeclinedEmail(MessageNotification<OrderSubmitEventBody> messageNotification);

        Task SendLineItemStatusChangeEmail(HSOrder order, LineItemStatusChanges lineItemStatusChanges, List<HSLineItem> lineItems, string firstName, string lastName, string email, EmailDisplayText lineItemEmailDisplayText);

        Task SendLineItemStatusChangeEmailMultipleRcpts(HSOrder order, LineItemStatusChanges lineItemStatusChanges, List<HSLineItem> lineItems, List<string> tos, EmailDisplayText lineItemEmailDisplayText);

        Task SendContactSupplierAboutProductEmail(ContactSupplierBody template);

        Task EmailVoidAuthorizationFailedAsync(HSPayment payment, string transactionID, HSOrder order, ApiError apiError);

        Task EmailGeneralSupportQueue(SupportCase supportCase);

        Task SendQuotePriceConfirmationEmail(HSOrder order, HSLineItem lineItem, string buyerEmail);

        Task SendQuoteRequestConfirmationEmail(HSOrder order, HSLineItem lineItem, string buyerEmail);

        Task SendOrderReturnApprovedEmail(MessageNotification<OrderReturnEventBody> messageNotification);

        Task SendOrderReturnCompletedEmail(MessageNotification<OrderReturnEventBody> messageNotification);

        Task SendOrderReturnDeclinedEmail(MessageNotification<OrderReturnEventBody> messageNotification);

        Task SendOrderReturnSubmittedForApprovalEmail(MessageNotification<OrderReturnEventBody> messageNotification);

        Task SendOrderReturnSubmittedForYourApprovalEmail(MessageNotification<OrderReturnEventBody> messageNotification);

        Task SendOrderReturnSubmittedForYourApprovalHasBeenApprovedEmail(MessageNotification<OrderReturnEventBody> messageNotification);

        Task SendOrderReturnSubmittedForYourApprovalHasBeenDeclinedEmail(MessageNotification<OrderReturnEventBody> messageNotification);
    }
}
