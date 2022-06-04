using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Flurl.Http;
using Flurl.Http.Configuration;
using Headstart.API.Commands;
using Headstart.API.Commands.Crud;
using Headstart.API.Helpers;
using Headstart.Common;
using Headstart.Common.Commands;
using Headstart.Common.Models;
using Headstart.Common.Queries;
using Headstart.Common.Services;
using Headstart.Common.Services.CMS;
using Headstart.Common.Settings;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Alerts;
using OrderCloud.Integrations.Avalara;
using OrderCloud.Integrations.CardConnect;
using OrderCloud.Integrations.EasyPost;
using OrderCloud.Integrations.EasyPost.Models;
using OrderCloud.Integrations.Emails;
using OrderCloud.Integrations.EnvironmentSeed.Commands;
using OrderCloud.Integrations.ExchangeRates;
using OrderCloud.Integrations.Library;
using OrderCloud.Integrations.Library.Cosmos;
using OrderCloud.Integrations.Library.cosmos_repo;
using OrderCloud.Integrations.Library.Interfaces;
using OrderCloud.Integrations.RMAs.Repositories;
using OrderCloud.Integrations.SendGrid;
using OrderCloud.Integrations.Smarty;
using OrderCloud.Integrations.Taxation.Interfaces;
using OrderCloud.Integrations.TaxJar;
using OrderCloud.Integrations.Vertex;
using OrderCloud.Integrations.Zoho;
using OrderCloud.SDK;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;
using SendGrid;
using ITaxCalculator = OrderCloud.Integrations.Taxation.Interfaces.ITaxCalculator;
using ITaxCodesProvider = OrderCloud.Integrations.Taxation.Interfaces.ITaxCodesProvider;

namespace Headstart.API
{
    public class Startup
    {
        private readonly AppSettings settings;

        public Startup(AppSettings settings)
        {
            this.settings = settings;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public static void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.EnsureCosmosDbIsCreated();
            app.UseCatalystExceptionHandler();
            app.UseHttpsRedirection();
            app.UseRouting();
            app.UseCors("integrationcors");
            app.UseAuthorization();
            app.UseEndpoints(endpoints => endpoints.MapControllers());
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint($"/swagger/v1/swagger.json", $"Headstart API v1");
                c.RoutePrefix = string.Empty;
            });
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            var clientIDs = settings.OrderCloudSettings.ClientIDsWithAPIAccess.Split(",");

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

            var avalaraConfig = new AvalaraConfig()
            {
                BaseApiUrl = settings.AvalaraSettings.BaseApiUrl,
                AccountID = settings.AvalaraSettings.AccountID,
                LicenseKey = settings.AvalaraSettings.LicenseKey,
                CompanyCode = settings.AvalaraSettings.CompanyCode,
                CompanyID = settings.AvalaraSettings.CompanyID,
            };

            var currencyConfig = new BlobServiceConfig()
            {
                ConnectionString = settings.StorageAccountSettings.ConnectionString,
                Container = settings.StorageAccountSettings.BlobContainerNameExchangeRates,
            };
            var assetConfig = new BlobServiceConfig()
            {
                ConnectionString = settings.StorageAccountSettings.ConnectionString,
                Container = "assets",
                AccessType = BlobContainerPublicAccessType.Container,
            };

            var flurlClientFactory = new PerBaseUrlFlurlClientFactory();
            var orderCloudClient = new OrderCloudClient(new OrderCloudClientConfig
            {
                ApiUrl = settings.OrderCloudSettings.ApiUrl,
                AuthUrl = settings.OrderCloudSettings.ApiUrl,
                ClientId = settings.OrderCloudSettings.MiddlewareClientID,
                ClientSecret = settings.OrderCloudSettings.MiddlewareClientSecret,
                Roles = new[] { ApiRole.FullAccess },
            });

            AvalaraCommand avalaraCommand = null;
            VertexCommand vertexCommand = null;
            TaxJarCommand taxJarCommand = null;
            switch (settings.EnvironmentSettings.TaxProvider)
            {
                case TaxProvider.Avalara:
                    avalaraCommand = new AvalaraCommand(avalaraConfig, settings.EnvironmentSettings.Environment.ToString());
                    break;
                case TaxProvider.Taxjar:
                    taxJarCommand = new TaxJarCommand(settings.TaxJarSettings);
                    break;
                case TaxProvider.Vertex:
                    vertexCommand = new VertexCommand(settings.VertexSettings);
                    break;
                default:
                    break;
            }

            IEasyPostShippingService easyPostShippingService = null;
            switch (settings.EnvironmentSettings.ShippingProvider)
            {
                case ShippingProvider.Custom:
                    break;
                default:
                    easyPostShippingService = new EasyPostShippingService(new EasyPostConfig() { APIKey = settings.EasyPostSettings.APIKey }, settings.EasyPostSettings.FedexAccountId);
                    break;
            }

            services.AddMvc(o =>
             {
                 o.Filters.Add(new OrderCloud.Integrations.Library.Attributes.ValidateModelAttribute());
                 o.EnableEndpointRouting = false;
             })
            .ConfigureApiBehaviorOptions(o => o.SuppressModelStateInvalidFilter = true)
            .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
            .AddNewtonsoftJson(options =>
            {
                options.SerializerSettings.ContractResolver = new Newtonsoft.Json.Serialization.DefaultContractResolver();
                options.SerializerSettings.Converters.Add(new StringEnumConverter());
            });

            services
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
                .AddCors(o => o.AddPolicy("integrationcors", builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); }))
                .AddSingleton<ISimpleCache, LazyCacheService>() // Replace LazyCacheService with RedisService if you have multiple server instances.
                .AddOrderCloudUserAuth(opts => opts.AddValidClientIDs(clientIDs))
                .AddOrderCloudWebhookAuth(opts => opts.HashKey = settings.OrderCloudSettings.WebhookHashKey)
                .InjectCosmosStore<LogQuery, OrchestrationLog>(cosmosConfig)
                .InjectCosmosStore<ReportTemplateQuery, ReportTemplate>(cosmosConfig)
                .AddCosmosDb(settings.CosmosSettings.EndpointUri, settings.CosmosSettings.PrimaryKey, settings.CosmosSettings.DatabaseName, cosmosContainers)
                .Inject<IPortalService>()
                .Inject<ICheckoutIntegrationCommand>()
                .Inject<IShipmentCommand>()
                .Inject<IOrderCommand>()
                .Inject<IPaymentCommand>()
                .Inject<IOrderSubmitCommand>()
                .Inject<IEnvironmentSeedCommand>()
                .Inject<IHSProductCommand>()
                .Inject<ILineItemCommand>()
                .Inject<IMeProductCommand>()
                .Inject<IDiscountDistributionService>()
                .Inject<IHSCatalogCommand>()
                .Inject<IHSSupplierCommand>()
                .Inject<ICreditCardCommand>()
                .Inject<ISupportAlertService>()
                .Inject<ISupplierApiClientHelper>()
                .AddSingleton<IEmailServiceProvider, SendGridService>()
                .AddSingleton<ISendGridClient>(x => new SendGridClient(settings.SendgridSettings.ApiKey))
                .AddSingleton<IFlurlClientFactory>(x => flurlClientFactory)
                .AddSingleton<DownloadReportCommand>()
                .Inject<IRMARepo>()
                .Inject<IZohoClient>()
                .AddSingleton<IOMSService>(z => new ZohoService(
                    new ZohoClient(settings.ZohoSettings, flurlClientFactory),
                    orderCloudClient))
                .AddSingleton<IExchangeRatesClient>(x => new ExchangeRatesClient(flurlClientFactory))
                .AddSingleton<ICurrencyConversionService, ExchangeRatesService>()
                .AddSingleton<IAssetClient>(provider => new AssetClient(new OrderCloudIntegrationsBlobService(assetConfig), settings.StorageAccountSettings))
                .AddSingleton<ICurrencyConversionCommand>(provider =>
                    new ExchangeRatesCommand(orderCloudClient, new OrderCloudIntegrationsBlobService(currencyConfig), provider.GetService<ICurrencyConversionService>(), provider.GetService<ISimpleCache>()))
                .AddSingleton<ITaxCodesProvider>(provider =>
                {
                    return settings.EnvironmentSettings.TaxProvider switch
                    {
                        TaxProvider.Avalara => avalaraCommand,
                        TaxProvider.Taxjar => taxJarCommand,
                        TaxProvider.Vertex => new NotImplementedTaxCodesProvider(),
                        _ => avalaraCommand // Avalara is default
                    };
                })
                .AddSingleton<ITaxCalculator>(provider =>
                {
                    return settings.EnvironmentSettings.TaxProvider switch
                    {
                        TaxProvider.Avalara => avalaraCommand,
                        TaxProvider.Vertex => vertexCommand,
                        TaxProvider.Taxjar => taxJarCommand,
                        _ => avalaraCommand // Avalara is default
                    };
                })
                .AddSingleton<IShippingService>(provider =>
                {
                    return settings.EnvironmentSettings.ShippingProvider switch
                    {
                        _ => easyPostShippingService // EasyPost is default
                    };
                })
                .AddSingleton<IOrderCloudIntegrationsCardConnectService>(x => new OrderCloudIntegrationsCardConnectService(settings.CardConnectSettings, settings.EnvironmentSettings.Environment.ToString(), flurlClientFactory))
                .AddSingleton<IOrderCloudClient>(provider => orderCloudClient)
                .AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Headstart Middleware API Documentation", Version = "v1" });
                    c.SchemaFilter<SwaggerExcludeFilter>();

                    List<string> xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
                    xmlFiles.ForEach(xmlFile => c.IncludeXmlComments(xmlFile));
                })
                .AddSwaggerGenNewtonsoftSupport()
                .AddSmartyIntegration(settings.SmartyStreetSettings, orderCloudClient);

            var serviceProvider = services.BuildServiceProvider();
            services
                .AddApplicationInsightsTelemetry(new ApplicationInsightsServiceOptions
                {
                    EnableAdaptiveSampling = false, // retain all data
                    InstrumentationKey = settings.ApplicationInsightsSettings.InstrumentationKey,
                });

            ServicePointManager.DefaultConnectionLimit = int.MaxValue;

            ConfigureFlurl();
        }

        public void ConfigureFlurl()
        {
            // This adds retry logic for any api call that fails with a transient error (server errors, timeouts, or rate limiting requests)
            // Will retry up to 3 times using exponential backoff and jitter, a mean of 3 seconds wait time in between retries
            // https://github.com/App-vNext/Polly/wiki/Retry-with-jitter#more-complex-jitter
            var delay = Backoff.DecorrelatedJitterBackoffV2(medianFirstRetryDelay: TimeSpan.FromSeconds(3), retryCount: 3);
            var policy = HttpPolicyExtensions
                            .HandleTransientHttpError()
                            .OrResult(response => response.StatusCode == HttpStatusCode.TooManyRequests)
                            .WaitAndRetryAsync(delay);

            // Flurl setting for JSON serialization
            var jsonSettings = new JsonSerializerSettings();
            jsonSettings.Converters.Add(new Newtonsoft.Json.Converters.StringEnumConverter());

            // Flurl setting for request timeout
            var timeout = TimeSpan.FromSeconds(settings.FlurlSettings.TimeoutInSeconds == 0 ? 30 : settings.FlurlSettings.TimeoutInSeconds);

            FlurlHttp.Configure(settings =>
            {
                settings.HttpClientFactory = new PollyFactory(policy);
                settings.JsonSerializer = new NewtonsoftJsonSerializer(jsonSettings);
                settings.Timeout = timeout;
            });
        }
    }
}
