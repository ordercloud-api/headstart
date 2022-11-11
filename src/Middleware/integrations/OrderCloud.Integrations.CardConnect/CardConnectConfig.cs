namespace OrderCloud.Integrations.CardConnect
{
    public class CardConnectConfig
    {
        public string Site { get; set; }

        public string BaseUrl { get; set; }

        /// <summary>
        /// Authorization credentials
        /// </summary>
        public string Authorization { get; set; }

        public string MerchantID { get; set; }
    }
}
