using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using Flurl.Http;
using Flurl.Http.Configuration;
using Headstart.API.Commands;
using Headstart.Common;
using Headstart.Common.Commands;
using Headstart.Common.Extensions;
using Headstart.Common.Helpers;
using Headstart.Common.Models;
using Headstart.Common.Services;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Alerts;
using OrderCloud.Integrations.Avalara.Extensions;
using OrderCloud.Integrations.CardConnect.Extensions;
using OrderCloud.Integrations.CMS.Extensions;
using OrderCloud.Integrations.CosmosDB;
using OrderCloud.Integrations.CosmosDB.Extensions;
using OrderCloud.Integrations.EasyPost.Extensions;
using OrderCloud.Integrations.Emails.Extensions;
using OrderCloud.Integrations.EnvironmentSeed.Commands;
using OrderCloud.Integrations.EnvironmentSeed.Extensions;
using OrderCloud.Integrations.ExchangeRates.Extensions;
using OrderCloud.Integrations.Orchestration;
using OrderCloud.Integrations.Orchestration.Models;
using OrderCloud.Integrations.Portal;
using OrderCloud.Integrations.Reporting.Extensions;
using OrderCloud.Integrations.RMAs.Extensions;
using OrderCloud.Integrations.SendGrid.Extensions;
using OrderCloud.Integrations.Smarty.Extensions;
using OrderCloud.Integrations.TaxJar.Extensions;
using OrderCloud.Integrations.Vertex.Extensions;
using OrderCloud.Integrations.Zoho.Extensions;
using OrderCloud.SDK;
using Polly;
using Polly.Contrib.WaitAndRetry;
using Polly.Extensions.Http;

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

            services.AddMvc(o =>
             {
                 o.Filters.Add(new Headstart.Common.Attributes.ValidateModelAttribute());
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
                .AddCors(o => o.AddPolicy("integrationcors", builder => { builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader(); }))
                .AddSingleton<ISimpleCache, LazyCacheService>() // Replace LazyCacheService with RedisService if you have multiple server instances.
                .AddSingleton<IFlurlClientFactory, PerBaseUrlFlurlClientFactory>()

                // Settings
                .AddSingleton(x => settings.EnvironmentSettings)
                .AddSingleton(x => settings.OrderCloudSettings)
                .AddSingleton(x => settings.StorageAccountSettings)

                // Configure OrderCloud
                .InjectOrderCloud<IOrderCloudClient>(settings.OrderCloudSettings)
                .AddOrderCloudUserAuth(opts => opts.AddValidClientIDs(clientIDs))
                .AddOrderCloudWebhookAuth(opts => opts.HashKey = settings.OrderCloudSettings.WebhookHashKey)

                // Configure Cosmos
                .InjectCosmosStore<LogQuery, OrchestrationLog>(cosmosConfig)
                .AddCosmosDb(settings.CosmosSettings.EndpointUri, settings.CosmosSettings.PrimaryKey, settings.CosmosSettings.DatabaseName, cosmosContainers)

                // Commands
                .Inject<ICheckoutIntegrationCommand>()
                .Inject<IShipmentCommand>()
                .Inject<IOrderCommand>()
                .Inject<IPaymentCommand>()
                .Inject<IOrderSubmitCommand>()
                .Inject<IEnvironmentSeedCommand>()
                .Inject<IHSProductCommand>()
                .Inject<ILineItemCommand>()
                .Inject<IMeProductCommand>()
                .Inject<ICatalogCommand>()
                .Inject<ISupplierCommand>()
                .Inject<ICreditCardCommand>()
                .AddSingleton<ISupplierSyncCommand, GenericSupplierCommand>()

                // Services
                .Inject<IPortalService>()
                .Inject<IDiscountDistributionService>()
                .Inject<ISupportAlertService>()
                .Inject<ISupplierApiClientHelper>()

                // Translations Provider
                .AddDefaultTranslationsProvider(settings.StorageAccountSettings)

                // Tax Providers
                .AddAvalaraTaxProvider(settings.EnvironmentSettings, settings.AvalaraSettings)
                .AddTaxJarTaxProvider(settings.EnvironmentSettings, settings.TaxJarSettings)
                .AddVertexTaxProvider(settings.EnvironmentSettings, settings.VertexSettings)
                .AddMockTaxProvider()

                // CMS Providers
                .AddDefaultCMSProvider(settings.EnvironmentSettings, settings.StorageAccountSettings)

                // Email Providers
                .AddSendGridEmailServiceProvider(settings.EnvironmentSettings, settings.SendgridSettings, settings.UI)
                .AddDefaultEmailServiceProvider(settings.EnvironmentSettings)

                // Shipping Providers
                .AddEasyPostShippingProvider(settings.EnvironmentSettings, settings.EasyPostSettings)
                .AddMockShippingProvider()

                // Payment Providers
                .AddCardConnectCreditCartProcessor(settings.EnvironmentSettings, settings.CardConnectSettings)
                .AddMockCreditCardProcessor()

                // Address Validation Providers
                .AddSmartyAddressValidationProvider(settings.EnvironmentSettings, settings.SmartyStreetSettings)
                .AddDefaultAddressProvider()

                // Currency Conversion Providers
                .AddExchangeRatesCurrencyConversionProvider(settings.EnvironmentSettings, settings.StorageAccountSettings)

                // RMA Providers
                .AddDefaultRMAsProvider(settings.EnvironmentSettings)

                // OMS Providers
                .AddZohoOMSProvider(settings.EnvironmentSettings, settings.ZohoSettings)
                .AddDefaultOMSProvider()

                // Reporting Providers
                .AddDefaultReportingProvider(settings.EnvironmentSettings, cosmosConfig)

                // Documentation
                .AddSwaggerGen(c =>
                {
                    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Headstart Middleware API Documentation", Version = "v1" });
                    c.SchemaFilter<SwaggerExcludeFilter>();

                    List<string> xmlFiles = Directory.GetFiles(AppContext.BaseDirectory, "*.xml", SearchOption.TopDirectoryOnly).ToList();
                    xmlFiles.ForEach(xmlFile => c.IncludeXmlComments(xmlFile));
                })
                .AddSwaggerGenNewtonsoftSupport();

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
