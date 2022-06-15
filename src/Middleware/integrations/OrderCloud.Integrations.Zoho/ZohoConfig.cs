namespace OrderCloud.Integrations.Zoho
{
    public class ZohoConfig
    {
        public string AccessToken { get; set; }

        public string ApiUrl { get; set; } = "https://books.zoho.com/api/v3";

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string OrganizationID { get; set; }

        public bool PerformOrderSubmitTasks { get; set; }
    }
}
