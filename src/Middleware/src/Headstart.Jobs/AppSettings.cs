using Headstart.Common.Settings;
using Headstart.Jobs.Settings;
using OrderCloud.Integrations.AzureServiceBus;
using OrderCloud.Integrations.AzureStorage;
using OrderCloud.Integrations.CosmosDB;

namespace Headstart.Jobs
{
    public class AppSettings
    {
        public CosmosSettings CosmosSettings { get; set; } = new CosmosSettings();

        public EnvironmentSettings EnvironmentSettings { get; set; } = new EnvironmentSettings();

        public FlurlSettings FlurlSettings { get; set; } = new FlurlSettings();

        public JobSettings JobSettings { get; set; } = new JobSettings();

        public OrderCloudSettings OrderCloudSettings { get; set; } = new OrderCloudSettings();

        public ServiceBusSettings ServiceBusSettings { get; set; } = new ServiceBusSettings();

        public StorageAccountSettings StorageAccountSettings { get; set; } = new StorageAccountSettings();
    }
}
