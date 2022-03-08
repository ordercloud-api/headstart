using OrderCloud.Catalyst;
using Headstart.Models.Misc;
using Headstart.API.Commands;
using Microsoft.AspNetCore.Mvc;
using Headstart.Common.Services;

namespace Headstart.API.Controllers
{
	/// <summary>
	/// OrderCloud message senders are a feature that helps to deliver event driven notifications to users.
	/// Think of them as advanced webhooks. If for instance you wanted to use webhooks to deliver a notification to all users
	/// who are able to approve a newly submitted order you would have to write code to cover the following steps:
	/// 1. A handler to receive the submit for approval webhook
	/// 2. take that high level order info and query the API for the rest of the needed order data
	/// 3. Query the API for all of the possible approving users (which can be complex)
	/// 4. Look up approving users' contact info
	/// 5. Send each message 
	/// Message senders take the hard work out of all that and will send one web request for each message that should be sent.
	/// TODO: explore moving ordersubmit and shipment created to message senders
	/// unless there's a good reason not to it would be good to have all messages firing from one centralized location
	/// </summary>
	[Route("messagesenders")]
	public class MessageSendersController
	{
		private readonly ISendgridService _sendgridService;
		private readonly IOrderCommand _orderCommand;

		/// <summary>
		/// The IOC based constructor method for the MessageSendersController class object with Dependency Injection
		/// </summary>
		/// <param name="sendgridService"></param>
		/// <param name="orderCommand"></param>
		public MessageSendersController(ISendgridService sendgridService, IOrderCommand orderCommand)
		{
			_sendgridService = sendgridService;
			_orderCommand = orderCommand;
		}

		/// <summary>
		/// New User submission action (POST method)
		/// </summary>
		/// <param name="payload"></param>
		[HttpPost, Route("newuserinvitation"), OrderCloudWebhookAuth]
		public async void HandleNewUser([FromBody] MessageNotification<PasswordResetEventBody> payload)
		{
			await _sendgridService.SendNewUserEmail(payload);
		}

		/// <summary>
		/// New User submission action (POST method)
		/// </summary>
		/// <param name="payload"></param>
		[HttpPost, Route("forgottenpassword"), OrderCloudWebhookAuth]
		public async void HandlePasswordReset([FromBody] MessageNotification<PasswordResetEventBody> payload)
		{
			await _sendgridService.SendPasswordResetEmail(payload);
		}

		/// <summary>
		/// Order Submitted for Approval action (POST method)
		/// </summary>
		/// <param name="payload"></param>
		[HttpPost, Route("ordersubmittedforapproval"), OrderCloudWebhookAuth]
		public async void HandleOrderSubmittedForApproval([FromBody] MessageNotification<OrderSubmitEventBody> payload)
		{
			await _sendgridService.SendOrderSubmittedForApprovalEmail(payload);
		}

		/// <summary>
		/// Order Requires Approval submission action (POST method)
		/// </summary>
		/// <param name="payload"></param>
		[HttpPost, Route("ordersubmittedforyourapproval"), OrderCloudWebhookAuth]
		public async void HandleOrderRequiresApproval([FromBody] MessageNotification<OrderSubmitEventBody> payload)
		{
			await _orderCommand.PatchOrderRequiresApprovalStatus(payload.EventBody.Order.ID);
			await _sendgridService.SendOrderRequiresApprovalEmail(payload);
		}

		/// <summary>
		/// Order Approved submission action (POST method)
		/// </summary>
		/// <param name="payload"></param>
		[HttpPost, Route("OrderApproved"), OrderCloudWebhookAuth]
		public async void HandleOrderApproved([FromBody] MessageNotification<OrderSubmitEventBody> payload)
		{
			await _sendgridService.SendOrderApprovedEmail(payload);
		}

		/// <summary>
		/// Order Declined submission action (POST method)
		/// </summary>
		/// <param name="payload"></param>
		[HttpPost, Route("orderdeclined"), OrderCloudWebhookAuth]
		public async void HandleOrderDeclined([FromBody] MessageNotification<OrderSubmitEventBody> payload)
		{
			await _sendgridService.SendOrderDeclinedEmail(payload);
		}
	}
}