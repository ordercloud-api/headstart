namespace Headstart.Common.Settings
{
    public enum AppEnvironment
    {
        Test,
        Staging,
        Production,
    }

    public class EnvironmentSettings
    {
        public string BuildNumber { get; set; } // set during deploy

        public string Commit { get; set; } // set during deploy

        public AppEnvironment Environment { get; set; }

        public string MiddlewareBaseUrl { get; set; }

        public string AddressValidationProvider { get; set; }

        public string CMSProvider { get; set; }

        public string CurrencyConversionProvider { get; set; }

        public string EmailServiceProvider { get; set; }

        public string OMSProvider { get; set; }

        public string ShippingProvider { get; set; }

        public string TaxProvider { get; set; }
    }
}
