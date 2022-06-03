namespace OrderCloud.Integrations.Zoho.Models
{
    public class ZohoTokenResponse
    {
        public string access_token { get; set; }

        public string api_domain { get; set; }

        public string token_type { get; set; }

        public double expires_in { get; set; }
    }
}
