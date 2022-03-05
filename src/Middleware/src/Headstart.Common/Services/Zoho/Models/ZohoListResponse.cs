using Newtonsoft.Json;

namespace Headstart.Common.Services.Zoho.Models
{
    public class ZohoListResponse
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }

        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; } = string.Empty;

        [JsonProperty(PropertyName = "page_context")]
        public ZohoPageContext Meta { get; set; } = new ZohoPageContext();
    }

    public class ZohoFilter
    {
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;
    }
}