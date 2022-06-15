namespace OrderCloud.Integrations.CardConnect
{
    public class CardConnectConfig
    {
        public string Site { get; set; }

        public string BaseUrl { get; set; }

        public string Authorization { get; set; }

        public string AuthorizationCad { get; set; } // we need a separate merchant account for canadian currency

        public string UsdMerchantID { get; set; }

        public string CadMerchantID { get; set; }

        public string EurMerchantID { get; set; }
    }
}
