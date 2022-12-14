namespace OrderCloud.Integrations.CardConnect
{
    public class CardConnectSettings
    {
        /// <summary>
        /// Partner level grouping. (eg: http://{site}.cardconnect.com), if applicable.
        /// </summary>
        public string Site { get; set; }

        /// <summary>
        /// URL used to connect to CardConnect (eg: cardconnect.com).
        /// </summary>
        public string BaseUrl { get; set; }

        /// <summary>
        /// CardConnect specific token for authorization - Required if EnvironmentSettings:PaymentProvider is set to "CardConnect".
        /// </summary>
        public string Authorization { get; set; }

        /// <summary>
        /// Merchant ID for CardConnect - Required if EnvironmentSettings:PaymentProvider is set to "CardConnect".
        /// </summary>
        public string MerchantID { get; set; }
    }
}
