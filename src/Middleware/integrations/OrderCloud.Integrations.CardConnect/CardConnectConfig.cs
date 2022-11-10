namespace OrderCloud.Integrations.CardConnect
{
    public class CardConnectConfig
    {
        public string Site { get; set; }

        public string BaseUrl { get; set; }

        /// <summary>
        /// Authorization credentials for US currency.
        /// </summary>
        public string Authorization { get; set; }

        /// <summary>
        /// Authorization credentials for Canadian currency.
        /// </summary>
        public string AuthorizationCad { get; set; }

        public string UsdMerchantID { get; set; }

        public string CadMerchantID { get; set; }

        public string EurMerchantID { get; set; }
    }
}
