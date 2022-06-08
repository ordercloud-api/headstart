using Headstart.API.Commands;
using Headstart.Models.Misc;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Emails;

namespace Headstart.Common.Controllers
{
    /// <summary>
    /// OrderCloud message senders are a feature that helps to deliver event driven notifications to users.
    /// Think of them as advanced webhooks. If for instance you wanted to use webhooks to deliver a notification to all users
    /// who are able to approve a newly submitted order you would have to write code to cover the following steps:
    /// 1. A handler to recieve the submit for approval webhook
    /// 2. take that high level order info and query the API for the rest of the needed order data
    /// 3. Query the API for all of the possible approving users (which can be complex)
    /// 4. Look up approving users' contact info
    /// 5. Send each message
    ///
    /// Message senders take the hard work out of all that and will send one web request for each message that should be sent.
    /// </summary>
    // TODO: explore moving ordersubmit and shipmentcreated to message senders
    // unless there's a good reason not to it would be good to have all messages firing from one centralized location
    [Route("messagesenders")]
    public class MessageSendersController : CatalystController
    {
        private readonly IEmailServiceProvider emailServiceProvider;
        private readonly IOrderCommand orderCommand;

        public MessageSendersController(IEmailServiceProvider emailServiceProvider, IOrderCommand orderCommand)
        {
            this.emailServiceProvider = emailServiceProvider;
            this.orderCommand = orderCommand;
        }

        [HttpPost, Route("newuserinvitation")]
        [OrderCloudWebhookAuth]
        public async void HandleNewUser([FromBody] MessageNotification<PasswordResetEventBody> payload)
        {
            await emailServiceProvider.SendNewUserEmail(payload);
        }

        [HttpPost, Route("forgottenpassword")]
        [OrderCloudWebhookAuth]
        public async void HandlePasswordReset([FromBody] MessageNotification<PasswordResetEventBody> payload)
        {
            await emailServiceProvider.SendPasswordResetEmail(payload);
        }

        [HttpPost, Route("ordersubmittedforapproval")]
        [OrderCloudWebhookAuth]
        public async void HandleOrderSubmittedForApproval([FromBody] MessageNotification<OrderSubmitEventBody> payload)
        {
            await emailServiceProvider.SendOrderSubmittedForApprovalEmail(payload);
        }

        [HttpPost, Route("ordersubmittedforyourapproval")]
        [OrderCloudWebhookAuth]
        public async void HandleOrderRequiresApproval([FromBody] MessageNotification<OrderSubmitEventBody> payload)
        {
            await orderCommand.PatchOrderRequiresApprovalStatus(payload.EventBody.Order.ID);
            await emailServiceProvider.SendOrderRequiresApprovalEmail(payload);
        }

        [HttpPost, Route("OrderApproved")]
        [OrderCloudWebhookAuth]
        public async void HandleOrderApproved([FromBody] MessageNotification<OrderSubmitEventBody> payload)
        {
            await emailServiceProvider.SendOrderApprovedEmail(payload);
        }

        [HttpPost, Route("orderdeclined")]
        [OrderCloudWebhookAuth]
        public async void HandleOrderDeclined([FromBody] MessageNotification<OrderSubmitEventBody> payload)
        {
            await emailServiceProvider.SendOrderDeclinedEmail(payload);
        }
    }
}
