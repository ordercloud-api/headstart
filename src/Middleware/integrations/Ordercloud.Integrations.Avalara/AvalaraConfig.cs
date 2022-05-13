namespace OrderCloud.Integrations.Avalara
{
    public class AvalaraConfig
    {
        public string BaseApiUrl { get; set; }

        public int AccountID { get; set; }

        public string LicenseKey { get; set; }

        public int CompanyID { get; set; }

        public string CompanyCode { get; set; }
    }
}
