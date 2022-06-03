using System;
using System.Collections.Generic;
using System.Net;
using Flurl.Http;
using Flurl.Http.Configuration;
using Headstart.API;
using Headstart.API.Commands;
using Headstart.API.Commands.Crud;
using Headstart.Common;
using Headstart.Common.Repositories;
using Headstart.Common.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using OrderCloud.Integrations.CardConnect;
using OrderCloud.Integrations.Emails;
using OrderCloud.Integrations.Library;
using OrderCloud.Integrations.Library.Cosmos;
using OrderCloud.Integrations.SendGrid;
using OrderCloud.Integrations.Zoho;
using OrderCloud.SDK;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using SendGrid;

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
                .AddSingleton(x => settings.ApplicationInsightsSettings)
                .AddSingleton(x => settings.AvalaraSettings)
                .AddSingleton(x => settings.CardConnectSettings)
                .AddSingleton(x => settings.CosmosSettings)
                .AddSingleton(x => settings.EasyPostSettings)
                .AddSingleton(x => settings.EnvironmentSettings)
                .AddSingleton(x => settings.FlurlSettings)
                .AddSingleton(x => settings.JobSettings)
                .AddSingleton(x => settings.OrderCloudSettings)
                .AddSingleton(x => settings.SendgridSettings)
                .AddSingleton(x => settings.ServiceBusSettings)
                .AddSingleton(x => settings.SmartyStreetSettings)
                .AddSingleton(x => settings.StorageAccountSettings)
                .AddSingleton(x => settings.TaxJarSettings)
                .AddSingleton(x => settings.UI)
                .AddSingleton(x => settings.VertexSettings)
                .AddSingleton(x => settings.ZohoSettings)
                .InjectOrderCloud<IOrderCloudClient>(new OrderCloudClientConfig()
                {
                    ApiUrl = settings.OrderCloudSettings.ApiUrl,
                    AuthUrl = settings.OrderCloudSettings.ApiUrl,
                    ClientId = settings.OrderCloudSettings.MiddlewareClientID,
                    ClientSecret = settings.OrderCloudSettings.MiddlewareClientSecret,
                    Roles = new[]
                    {
                        ApiRole.FullAccess,
                    },
                })
                .AddCosmosDb(settings.CosmosSettings.EndpointUri, settings.CosmosSettings.PrimaryKey, settings.CosmosSettings.DatabaseName, cosmosContainers)
                .AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>()
                .AddSingleton<IOrderCloudIntegrationsCardConnectService>(x => new OrderCloudIntegrationsCardConnectService(settings.CardConnectSettings, settings.EnvironmentSettings.Environment.ToString(), flurlClientFactory))
                .Inject<IHSCatalogCommand>()
                .Inject<IHSBuyerLocationCommand>()
                .AddSingleton<PaymentCaptureJob>()
                .AddSingleton<SendRecentOrdersJob>()
                .AddSingleton<ReceiveRecentSalesOrdersJob>()
                .AddSingleton<ReceiveProductDetailsJob>()
                .AddSingleton<ReceiveRecentPurchaseOrdersJob>()
                .AddSingleton<ReceiveRecentLineItemsJob>()
                .AddSingleton<ReceiveRecentOrdersAndShipmentsJob>()
                .Inject<IZohoClient>()
                .AddSingleton<IOMSService, ZohoService>()
                .AddSingleton<ISendGridClient>(x => new SendGridClient(settings.SendgridSettings.ApiKey))
                .AddSingleton<IEmailServiceProvider, SendGridService>()
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
