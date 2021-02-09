using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Common.Services.Zoho
{
    public class ZohoClientConfig
    {
        public string AccessToken { get; set; }
        public string OrganizationID { get; set; }
        public string ApiUrl { get; set; } = "https://books.zoho.com/api/v3";
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
    }
}
