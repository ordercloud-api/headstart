using Headstart.Common.Settings;
using OrderCloud.Integrations.Avalara;
using OrderCloud.Integrations.AzureStorage;
using OrderCloud.Integrations.CardConnect;
using OrderCloud.Integrations.CosmosDB;
using OrderCloud.Integrations.EasyPost;
using OrderCloud.Integrations.ExchangeRates;
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

        public CardConnectSettings CardConnectSettings { get; set; } = new CardConnectSettings();

        public CosmosSettings CosmosSettings { get; set; } = new CosmosSettings();

        public EasyPostSettings EasyPostSettings { get; set; } = new EasyPostSettings();

        public EnvironmentSettings EnvironmentSettings { get; set; } = new EnvironmentSettings();

        public ExchangeRateSettings ExchangeRateSettings { get; set; } = new ExchangeRateSettings();

        public FlurlSettings FlurlSettings { get; set; } = new FlurlSettings();

        public OrderCloudSettings OrderCloudSettings { get; set; } = new OrderCloudSettings();

        public SendGridSettings SendgridSettings { get; set; } = new SendGridSettings();

        public SmartyStreetSettings SmartyStreetSettings { get; set; } = new SmartyStreetSettings();

        public StorageAccountSettings StorageAccountSettings { get; set; } = new StorageAccountSettings();

        public VertexSettings VertexSettings { get; set; } = new VertexSettings();

        public TaxJarSettings TaxJarSettings { get; set; } = new TaxJarSettings();

        public UI UI { get; set; }

        public ZohoSettings ZohoSettings { get; set; } = new ZohoSettings();
    }
}
