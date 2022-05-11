using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace Headstart.Common.Services.Zoho.Models
{
    public class ZohoListResponse
    {
        [JsonProperty(PropertyName = "code")]
        public int Code { get; set; }
        [JsonProperty(PropertyName = "message")]
        public string Message { get; set; }
        [JsonProperty(PropertyName = "page_context")]
        public ZohoPageContext Meta { get; set; }
    }

    public class ZohoFilter
    {
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
