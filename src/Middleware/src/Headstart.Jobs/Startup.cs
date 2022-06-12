using System;
using System.Collections.Generic;
using System.Net;
using Flurl.Http;
using Flurl.Http.Configuration;
using Headstart.Common;
using Headstart.Common.Commands;
using Headstart.Common.Extensions;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using OrderCloud.Integrations.CosmosDB;
using OrderCloud.Integrations.CosmosDB.Extensions;
using OrderCloud.Integrations.Reporting.Repositories;
using OrderCloud.SDK;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

[assembly: FunctionsStartup(typeof(Headstart.Jobs.Startup))]

namespace Headstart.Jobs
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var connectionString = Environment.GetEnvironmentVariable("APP_CONFIG_CONNECTION");
            var settings = new AppSettings();
            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddAzureAppConfiguration(connectionString)
                .Build();
            config.Bind(settings);

            var cosmosConfig = new CosmosConfig(
                settings.CosmosSettings.DatabaseName,
                settings.CosmosSettings.EndpointUri,
                settings.CosmosSettings.PrimaryKey,
                settings.CosmosSettings.RequestTimeoutInSeconds);

            var cosmosContainers = new List<ContainerInfo>()
            {
                new ContainerInfo()
                {
                    Name = "salesorderdetail",
                    PartitionKey = "/PartitionKey",
                },
                new ContainerInfo()
                {
                    Name = "purchaseorderdetail",
                    PartitionKey = "/PartitionKey",
                },
                new ContainerInfo()
                {
                    Name = "lineitemdetail",
                    PartitionKey = "/PartitionKey",
                },
                new ContainerInfo()
                {
                    Name = "rmas",
                    PartitionKey = "/PartitionKey",
                },
                new ContainerInfo()
                {
                    Name = "shipmentdetail",
                    PartitionKey = "/PartitionKey",
                },
                new ContainerInfo()
                {
                    Name = "productdetail",
                    PartitionKey = "/PartitionKey",
                },
            };

            // https://github.com/Polly-Contrib/Polly.Contrib.WaitAndRetry#wait-and-retry-with-jittered-back-off
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(30), retryCount: 3);
            var policy = HttpPolicyExtensions
                            .HandleTransientHttpError()
                            .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
                            .WaitAndRetryAsync(delay);

            var flurlClientFactory = new PerBaseUrlFlurlClientFactory();

            FlurlHttp.Configure(settings => settings.HttpClientFactory = new PollyFactory(policy));

            builder.Services
                .AddSingleton(x => settings.OrderCloudSettings)
                .AddSingleton(x => settings.ServiceBusSettings)
                .AddSingleton(x => settings.StorageAccountSettings)
                .InjectOrderCloud<IOrderCloudClient>(settings.OrderCloudSettings)
                .AddCosmosDb(settings.CosmosSettings.EndpointUri, settings.CosmosSettings.PrimaryKey, settings.CosmosSettings.DatabaseName, cosmosContainers)
                .AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>()
                .Inject<ICatalogCommand>()
                .Inject<IHSBuyerLocationCommand>()
                .AddSingleton<PaymentCaptureJob>()
                .AddSingleton<SendRecentOrdersJob>()
                .AddSingleton<ReceiveRecentSalesOrdersJob>()
                .AddSingleton<ReceiveProductDetailsJob>()
                .AddSingleton<ReceiveRecentPurchaseOrdersJob>()
                .AddSingleton<ReceiveRecentLineItemsJob>()
                .AddSingleton<ReceiveRecentOrdersAndShipmentsJob>()
                .Inject<ISalesOrderDetailDataRepo>()
                .Inject<IPurchaseOrderDetailDataRepo>()
                .Inject<ILineItemDetailDataRepo>()
                .Inject<IOrdersAndShipmentsDataRepo>()
                .AddSingleton(settings)
                .AddMvcCore().AddNewtonsoftJson(o =>
                {
                    o.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    o.SerializerSettings.Converters.Add(new StringEnumConverter());
                    o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                })
                .Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config));
        }
    }
}
