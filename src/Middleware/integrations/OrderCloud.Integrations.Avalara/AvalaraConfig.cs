namespace OrderCloud.Integrations.Avalara
{
    public class AvalaraSettings
    {
        /// <summary>
        /// AccountId associated to Avalara account - Required if EnvironmentSettings:TaxCodeProvider is set to Avalara.
        /// </summary>
        public int AccountID { get; set; }

        /// <summary>
        /// Avalara Url to use based on environment (eg: https://rest.avatax.com/api/v2, https://sandbox-rest.avatax.com/api/v2) - Required if EnvironmentSettings:TaxCodeProvider is set to Avalara.
        /// </summary>
        public string BaseApiUrl { get; set; }

        /// <summary>
        /// ComapanyID associated with Avalara account - Required if EnvironmentSettings:TaxCodeProvider is set to Avalara.
        /// </summary>
        public int CompanyID { get; set; }

        /// <summary>
        /// Company code associated with Avalara account - Required if EnvironmentSettings:TaxCodeProvider is set to Avalara.
        /// </summary>
        public string CompanyCode { get; set; }

        /// <summary>
        /// License Key associated with Avalara account - Required if EnvironmentSettings:TaxCodeProvider is set to Avalara.
        /// </summary>
        public string LicenseKey { get; set; }
    }
}
