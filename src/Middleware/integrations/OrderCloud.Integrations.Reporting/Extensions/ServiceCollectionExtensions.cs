using Headstart.Common.Extensions;
using Headstart.Common.Settings;
using Microsoft.Extensions.DependencyInjection;
using OrderCloud.Integrations.CosmosDB;
using OrderCloud.Integrations.CosmosDB.Extensions;
using OrderCloud.Integrations.Reporting.Commands;
using OrderCloud.Integrations.Reporting.Models;
using OrderCloud.Integrations.Reporting.Queries;
using OrderCloud.Integrations.Reporting.Repositories;

namespace OrderCloud.Integrations.Reporting.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultReportingProvider(this IServiceCollection services, EnvironmentSettings environmentSettings, CosmosConfig cosmosConfig)
        {
            services
                .AddSingleton<IDownloadReportCommand, DownloadReportCommand>()
                .InjectCosmosStore<ReportTemplateQuery, ReportTemplate>(cosmosConfig)
                .AddDefaultReportingRepositories();

            return services;
        }

        public static IServiceCollection AddDefaultReportingRepositories(this IServiceCollection services)
        {
            services
                .Inject<ISalesOrderDetailDataRepo>()
                .Inject<IPurchaseOrderDetailDataRepo>()
                .Inject<ILineItemDetailDataRepo>()
                .Inject<IOrdersAndShipmentsDataRepo>()
                .Inject<IProductDetailDataRepo>();

            return services;
        }
    }
}
