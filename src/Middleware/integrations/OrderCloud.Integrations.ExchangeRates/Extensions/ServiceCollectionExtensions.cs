using System;
using Headstart.Common.Commands;
using Headstart.Common.Services;
using Headstart.Common.Settings;
using Microsoft.Extensions.DependencyInjection;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.AzureStorage;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.ExchangeRates.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddExchangeRatesCurrencyConversionProvider(this IServiceCollection services, EnvironmentSettings environmentSettings, StorageAccountSettings storageAccountSettings)
        {
            // Currently ExchangeRates is the only CurrencyConversionProvider and a setting with a value other than ExchangeRates will error
            // when creating a product (among other places) because it isn't able to resolve a required service
            // There is already some default functionality included whereby static 1:1 rates are returned
            // so I am commenting out the following for now. If another provider is eventually included then this switch can be added back
            // if (!environmentSettings.CurrencyConversionProvider.Equals("ExchangeRates", StringComparison.OrdinalIgnoreCase))
            // {
            //    return services;
            // }
            var currencyConfig = new CloudBlobServiceConfig()
            {
                ConnectionString = storageAccountSettings.ConnectionString,
                Container = storageAccountSettings.BlobContainerNameExchangeRates,
            };

            services
                .AddSingleton<ICurrencyConversionCommand>(provider => new ExchangeRatesCommand(
                    provider.GetService<IOrderCloudClient>(),
                    new CloudBlobService(currencyConfig),
                    provider.GetService<ICurrencyConversionService>(),
                    provider.GetService<ISimpleCache>()))
                .AddSingleton<IExchangeRatesClient, ExchangeRatesClient>()
                .AddSingleton<ICurrencyConversionService, ExchangeRatesService>();

            return services;
        }
    }
}
