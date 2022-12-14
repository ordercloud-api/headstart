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
        /// <summary>
        /// The build number, automatically set during deploy
        /// </summary>
        public string BuildNumber { get; set; }

        /// <summary>
        /// The commit, automatically set during deploy.
        /// </summary>
        public string Commit { get; set; }

        /// <summary>
        /// The current environment (Test, UAT, Production).
        /// </summary>
        public AppEnvironment Environment { get; set; }

        /// <summary>
        /// Base URL to the middleware API.
        /// </summary>
        public string MiddlewareBaseUrl { get; set; }

        /// <summary>
        /// Which address validation service provider will be used where address validation has been requested.
        /// Options include: "Smarty". Otherwise no validation will be performed.
        /// </summary>
        public string AddressValidationProvider { get; set; } = string.Empty;

        /// <summary>
        /// Which content management system provider will be used for storing images and pdfs for product information.
        /// Options include: "N/A". Otherwise will default to custom integration with Azure blob storage.
        /// </summary>
        public string CMSProvider { get; set; } = string.Empty;

        /// <summary>
        /// Which currency conversion provider should be used for currency conversion.
        /// Options include: "ExchangeRates". Otherwise will use mocked currency conversions.
        /// </summary>
        public string CurrencyConversionProvider { get; set; } = string.Empty;

        /// <summary>
        /// Which email service provider should be used for emails.
        /// Options include: "SendGrid". Otherwise no emails will be sent.
        /// </summary>
        public string EmailServiceProvider { get; set; } = string.Empty;

        /// <summary>
        /// Which order management service provider should be used for exporting orders to.
        /// Options include: "Zoho". Otherwise orders will not be forwarded to an OMS.
        /// </summary>
        public string OMSProvider { get; set; } = string.Empty;

        /// <summary>
        /// Which payment service provider should be used for payments.
        /// Options include: "CardConnect". Otherwise all payments and transactions will be mocked.
        /// </summary>
        public string PaymentProvider { get; set; } = string.Empty;

        /// <summary>
        /// Which shipping service provider should be used for shipping methods and fees calculation. Options include: "EasyPost". Otherwise shipping estimates will be mocked.
        /// </summary>
        public string ShippingProvider { get; set; } = string.Empty;

        /// <summary>
        /// Which tax service should be used for tax calculation.
        /// Options include: "Avalara", "Taxjar", or "Vertex". Otherwise mock responses will be returned from the DefaultTaxProvider implementation.
        /// </summary>
        public string TaxProvider { get; set; } = string.Empty;
    }
}
