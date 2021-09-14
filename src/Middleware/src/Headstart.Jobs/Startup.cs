using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using Flurl.Http;
using Headstart.Jobs;
using Flurl.Http.Configuration;
using Headstart.API.Commands;
using Headstart.API.Commands.Crud;
using Headstart.Common;
using Headstart.Common.Models;
using Headstart.Common.Queries;
using Headstart.Common.Services;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using ordercloud.integrations.cardconnect;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using OrderCloud.Catalyst;
using SendGrid;
using Polly;
using Polly.Extensions.Http;
using Headstart.Common.Services.Zoho;
using Microsoft.Azure.WebJobs.ServiceBus;
using Headstart.API.Commands.Zoho;
using Polly.Contrib.WaitAndRetry;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Headstart.Common.Repositories;

[assembly: FunctionsStartup(typeof(Startup))]
namespace Headstart.Jobs
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var connectionString = Environment.GetEnvironmentVariable("APP_CONFIG_CONNECTION");

            var settings = builder
                .BuildSettingsFromAzureAppConfig<AppSettings>(connectionString);

            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddAzureAppConfiguration(connectionString ?? Environment.GetEnvironmentVariable("APP_CONFIG_CONNECTION"))
                .Build();

            var cosmosConfig = new CosmosConfig(
                settings.CosmosSettings.DatabaseName,
                settings.CosmosSettings.EndpointUri,
                settings.CosmosSettings.PrimaryKey,
                settings.CosmosSettings.RequestTimeoutInSeconds,
                settings.CosmosSettings.MaxConnectionLimit,
                settings.CosmosSettings.IdleTcpConnectionTimeoutInMinutes,
                settings.CosmosSettings.OpenTcpConnectionTimeoutInSeconds,
                settings.CosmosSettings.MaxTcpConnectionsPerEndpoint,
                settings.CosmosSettings.MaxRequestsPerTcpConnection,
                settings.CosmosSettings.EnableTcpConnectionEndpointRediscovery
            );

            var cosmosContainers = new List<ContainerInfo>()
            {
                new ContainerInfo()
                {
                    Name = "salesorderdetail",
                    PartitionKey = "PartitionKey"
                },
                new ContainerInfo()
                {
                    Name = "purchaseorderdetail",
                    PartitionKey = "PartitionKey"
                },
                new ContainerInfo()
                {
                    Name = "lineitemdetail",
                    PartitionKey = "PartitionKey"
                },
                new ContainerInfo()
                {
                    Name = "rmas",
                    PartitionKey = "PartitionKey"
                },
                new ContainerInfo()
                {
                    Name = "shipmentdetail",
                    PartitionKey = "PartitionKey"
                },
                new ContainerInfo()
                {
                    Name = "productdetail",
                    PartitionKey = "PartitionKey"
                }
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
                .Inject<IAnytimeDashboardClient>()
                .Inject<IWaxDashboardClient>()
                .InjectOrderCloud<IOrderCloudClient>(new OrderCloudClientConfig()
                {
                    ApiUrl = settings.OrderCloudSettings.ApiUrl,
                    AuthUrl = settings.OrderCloudSettings.ApiUrl,
                    ClientId = settings.OrderCloudSettings.ClientID,
                    ClientSecret = settings.OrderCloudSettings.ClientSecret,
                    Roles = new[]
                    {
                        ApiRole.FullAccess
                    }
                })
                .InjectCosmosStore<ResourceHistoryQuery<ProductHistory>, ProductHistory>(cosmosConfig)
                .InjectCosmosStore<ResourceHistoryQuery<PriceScheduleHistory>, PriceScheduleHistory>(cosmosConfig)
                .AddCosmosDb(settings.CosmosSettings.EndpointUri, settings.CosmosSettings.PrimaryKey, settings.CosmosSettings.DatabaseName, cosmosContainers)
                .AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>()
                .AddSingleton<IOrderCloudIntegrationsCardConnectService>(x => new OrderCloudIntegrationsCardConnectService(settings.CardConnectSettings, flurlClientFactory))
                .Inject<IProductUpdateJob>()
                .Inject<ISyncOrgCommand>()
                .Inject<IHSCatalogCommand>()
                .Inject<IHSBuyerLocationCommand>()
                .AddSingleton<PaymentCaptureJob>()
                .AddSingleton<ZohoJob>()
                .AddSingleton<SendRecentOrdersJob>()
                .AddSingleton<ReceiveRecentSalesOrdersJob>()
                .AddSingleton<ReceiveProductDetailsJob>()
                .AddSingleton<ReceiveRecentPurchaseOrdersJob>()
                .AddSingleton<ReceiveRecentLineItemsJob>()
                .AddSingleton<ReceiveRecentOrdersAndShipmentsJob>()
                .AddSingleton(x => new ZohoClientConfig
                {
                    ApiUrl = "https://books.zoho.com/api/v3",
                    AccessToken = settings.ZohoSettings.AccessToken,
                    ClientId = settings.ZohoSettings.ClientId,
                    ClientSecret = settings.ZohoSettings.ClientSecret,
                    OrganizationID = settings.ZohoSettings.OrgID
                })
                .Inject<IZohoClient>()
                .Inject<IZohoCommand>()
                .AddSingleton<ISendGridClient>(x => new SendGridClient(settings.SendgridSettings.ApiKey))
                .Inject<ISendgridService>()
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