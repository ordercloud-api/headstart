using Headstart.Common.Settings;
using OrderCloud.Integrations.CardConnect;
using OrderCloud.Integrations.Library;
using OrderCloud.Integrations.Library.Cosmos;
using OrderCloud.Integrations.SendGrid;
using OrderCloud.Integrations.Smarty;
using OrderCloud.Integrations.TaxJar;
using OrderCloud.Integrations.Vertex;

namespace Headstart.API
{
    public enum AppEnvironment
    {
        Test,
        Staging,
        Production,
    }

    public enum ShippingProvider
    {
        EasyPost,
        Custom,
    }

    public enum TaxProvider
    {
        Avalara,
        Vertex,
        Taxjar,
    }

    public class AppSettings
    {
        public ApplicationInsightsSettings ApplicationInsightsSettings { get; set; } = new ApplicationInsightsSettings();

        public AvalaraSettings AvalaraSettings { get; set; } = new AvalaraSettings();

        public CosmosSettings CosmosSettings { get; set; } = new CosmosSettings();

        public EasyPostSettings EasyPostSettings { get; set; } = new EasyPostSettings();

        public EnvironmentSettings EnvironmentSettings { get; set; } = new EnvironmentSettings();

        public FlurlSettings FlurlSettings { get; set; } = new FlurlSettings();

        public JobSettings JobSettings { get; set; } = new JobSettings();

        public OrderCloudSettings OrderCloudSettings { get; set; } = new OrderCloudSettings();

        public OrderCloudIntegrationsCardConnectConfig CardConnectSettings { get; set; } = new OrderCloudIntegrationsCardConnectConfig();

        public SendGridSettings SendgridSettings { get; set; } = new SendGridSettings();

        public ServiceBusSettings ServiceBusSettings { get; set; } = new ServiceBusSettings();

        public SmartyStreetsConfig SmartyStreetSettings { get; set; } = new SmartyStreetsConfig();

        public VertexConfig VertexSettings { get; set; } = new VertexConfig();

        public TaxJarConfig TaxJarSettings { get; set; } = new TaxJarConfig();

        public StorageAccountSettings StorageAccountSettings { get; set; } = new StorageAccountSettings();

        public UI UI { get; set; }

        public ZohoSettings ZohoSettings { get; set; } = new ZohoSettings();
    }

    public class ApplicationInsightsSettings
    {
        public string InstrumentationKey { get; set; }
    }

    public class AvalaraSettings
    {
        public int AccountID { get; set; }

        public string BaseApiUrl { get; set; }

        public string CompanyCode { get; set; }

        public int CompanyID { get; set; }

        public string LicenseKey { get; set; }
    }

    public class EasyPostSettings
    {
        public string APIKey { get; set; }

        public string FedexAccountId { get; set; }

        public int FreeShippingTransitDays { get; set; }

        public decimal NoRatesFallbackCost { get; set; }

        public int NoRatesFallbackTransitDays { get; set; }

        public string USPSAccountId { get; set; }
    }

    public class EnvironmentSettings
    {
        public string BuildNumber { get; set; } // set during deploy

        public string Commit { get; set; } // set during deploy

        public AppEnvironment Environment { get; set; }

        public string MiddlewareBaseUrl { get; set; }

        public ShippingProvider ShippingProvider { get; set; } = ShippingProvider.EasyPost;

        public TaxProvider TaxProvider { get; set; } = TaxProvider.Avalara;
    }

    public class FlurlSettings
    {
        public int TimeoutInSeconds { get; set; }
    }

    public class JobSettings
    {
        public string CaptureCreditCardsAfterDate { get; set; } // TODO: remove this once all orders have IsPaymentCaptured set

        public bool ShouldCaptureCreditCardPayments { get; set; }

        public bool ShouldRunZoho { get; set; }
    }

    public class ZohoSettings
    {
        public string AccessToken { get; set; }

        public string ClientId { get; set; }

        public string ClientSecret { get; set; }

        public string OrgID { get; set; }

        public bool PerformOrderSubmitTasks { get; set; }
    }
}
