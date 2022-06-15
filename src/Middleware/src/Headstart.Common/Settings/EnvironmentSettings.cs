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

        public string AddressValidationProvider { get; set; } = string.Empty;

        public string CMSProvider { get; set; } = string.Empty;

        public string CurrencyConversionProvider { get; set; } = string.Empty;

        public string EmailServiceProvider { get; set; } = string.Empty;

        public string OMSProvider { get; set; } = string.Empty;

        public string PaymentProvider { get; set; } = string.Empty;

        public string ShippingProvider { get; set; } = string.Empty;

        public string TaxProvider { get; set; } = string.Empty;
    }
}
