using OrderCloud.SDK;

namespace Headstart.Models.Misc
{
    public class WebhookResponse<T> : WebhookResponse
    {
        public WebhookResponse(T body)
        {
            Body = body;
            Message = "Unspecified error in webhook";
        }

        public string Message { get; set; }

        public T Body { get; set; }
    }
}
