namespace OrderCloud.Integrations.Zoho
{
    public class ZohoSettings
    {
        /// <summary>
        /// AccessToken used with Zoho
        /// Required if EnvironmentSettings:OMSProvider is set to "Zoho".
        /// </summary>
        public string AccessToken { get; set; }

        /// <summary>
        /// Zoho's API URL
        /// Defaults to https://books.zoho.com/api/v3.
        /// </summary>
        public string ApiUrl { get; set; } = "https://books.zoho.com/api/v3";

        /// <summary>
        /// ClientID used with Zoho
        /// Required if EnvironmentSettings:OMSProvider is set to "Zoho".
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// ClientSecret used with Zoho and specified ClientID
        /// Required if EnvironmentSettings:OMSProvider is set to "Zoho".
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// OrgID used with Zoho
        /// Required if EnvironmentSettings:OMSProvider is set to "Zoho".
        /// </summary>
        public string OrganizationID { get; set; }

        /// <summary>
        /// Boolean that determines if order submit tasks should go through Zoho
        /// Required if EnvironmentSettings:OMSProvider is set to "Zoho".
        /// </summary>
        public bool PerformOrderSubmitTasks { get; set; }
    }
}
