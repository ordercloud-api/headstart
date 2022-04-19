using OrderCloud.SDK;

namespace Headstart.Common.Models.Misc
{
	public class WebhookResponse<T> : WebhookResponse
	{
		public string Message { get; set; } = string.Empty;

		public T Body { get; set; }

		public WebhookResponse(T body)
		{
			Body = body;
			Message = @"Unspecified error in webhook.";
		}
	}
}