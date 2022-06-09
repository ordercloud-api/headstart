using Headstart.Common.Settings;
using OrderCloud.Integrations.AzureServiceBus;
using OrderCloud.Integrations.AzureStorage;
using OrderCloud.Integrations.CardConnect;
using OrderCloud.Integrations.CosmosDB;
using OrderCloud.Integrations.EasyPost;
using OrderCloud.Integrations.SendGrid;
using OrderCloud.Integrations.Smarty;
using OrderCloud.Integrations.TaxJar;
using OrderCloud.Integrations.Vertex;
using OrderCloud.Integrations.Zoho;

namespace Headstart.API
{
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

        public CardConnectConfig CardConnectSettings { get; set; } = new CardConnectConfig();

        public SendGridSettings SendgridSettings { get; set; } = new SendGridSettings();

        public ServiceBusSettings ServiceBusSettings { get; set; } = new ServiceBusSettings();

        public SmartyStreetsConfig SmartyStreetSettings { get; set; } = new SmartyStreetsConfig();

        public VertexConfig VertexSettings { get; set; } = new VertexConfig();

        public TaxJarConfig TaxJarSettings { get; set; } = new TaxJarConfig();

        public StorageAccountSettings StorageAccountSettings { get; set; } = new StorageAccountSettings();

        public UI UI { get; set; }

        public ZohoConfig ZohoSettings { get; set; } = new ZohoConfig();
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
}
