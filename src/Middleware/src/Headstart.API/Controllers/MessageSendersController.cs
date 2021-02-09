using Headstart.Common.Services;
using Microsoft.AspNetCore.Mvc;
using Headstart.Models.Misc;
using ordercloud.integrations.library;
using Headstart.API.Commands;

namespace Headstart.Common.Controllers
{
    public class MessageSendersController
    {
        private readonly ISendgridService _sendgridService;
        private readonly IOrderCommand _orderCommand;

        public MessageSendersController(ISendgridService sendgridService, IOrderCommand orderCommand)
        {
            _sendgridService = sendgridService;
            _orderCommand = orderCommand;
        }

        [HttpPost, Route("newuser")] // TO DO: send email to mp manager
        [OrderCloudWebhookAuth]
        public async void HandleNewUser([FromBody] MessageNotification<PasswordResetEventBody> payload)
        {
            await _sendgridService.SendNewUserEmail(payload);
        }

        [HttpPost, Route("passwordreset")]
        [OrderCloudWebhookAuth]
        public async void HandleBuyerPasswordReset([FromBody] MessageNotification<PasswordResetEventBody> payload)
        {
            await _sendgridService.SendPasswordResetEmail(payload);
        }

        [HttpPost, Route("ordersubmittedforapproval")]
        [OrderCloudWebhookAuth]
        public async void HandleOrderSubmittedForApproval([FromBody] MessageNotification<OrderSubmitEventBody> payload)
        {
            await _sendgridService.SendOrderSubmittedForApprovalEmail(payload);
        }

        [HttpPost, Route("orderrequiresapproval")]
        [OrderCloudWebhookAuth]
        public async void HandleOrderRequiresApproval([FromBody] MessageNotification<OrderSubmitEventBody> payload)
        {
            await _orderCommand.PatchOrderRequiresApprovalStatus(payload.EventBody.Order.ID);
            await _sendgridService.SendOrderRequiresApprovalEmail(payload);
        }

        [HttpPost, Route("orderisapproved")]
        [OrderCloudWebhookAuth]
        public async void HandleOrderApproved([FromBody] MessageNotification<OrderSubmitEventBody> payload)
        {
            await _sendgridService.SendOrderApprovedEmail(payload);
        }

        [HttpPost, Route("orderisdeclined")]
        [OrderCloudWebhookAuth]
        public async void HandleOrderDeclined([FromBody] MessageNotification<OrderSubmitEventBody> payload)
        {
            await _sendgridService.SendOrderDeclinedEmail(payload);
        }
    }
}