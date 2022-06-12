using Headstart.Common.Settings;
using OrderCloud.Integrations.Avalara;
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

        public AvalaraConfig AvalaraSettings { get; set; } = new AvalaraConfig();

        public CardConnectConfig CardConnectSettings { get; set; } = new CardConnectConfig();

        public CosmosSettings CosmosSettings { get; set; } = new CosmosSettings();

        public EasyPostSettings EasyPostSettings { get; set; } = new EasyPostSettings();

        public EnvironmentSettings EnvironmentSettings { get; set; } = new EnvironmentSettings();

        public FlurlSettings FlurlSettings { get; set; } = new FlurlSettings();

        public OrderCloudSettings OrderCloudSettings { get; set; } = new OrderCloudSettings();

        public SendGridSettings SendgridSettings { get; set; } = new SendGridSettings();

        public SmartyStreetsConfig SmartyStreetSettings { get; set; } = new SmartyStreetsConfig();

        public StorageAccountSettings StorageAccountSettings { get; set; } = new StorageAccountSettings();

        public VertexConfig VertexSettings { get; set; } = new VertexConfig();

        public TaxJarConfig TaxJarSettings { get; set; } = new TaxJarConfig();

        public UI UI { get; set; }

        public ZohoConfig ZohoSettings { get; set; } = new ZohoConfig();
    }
}
