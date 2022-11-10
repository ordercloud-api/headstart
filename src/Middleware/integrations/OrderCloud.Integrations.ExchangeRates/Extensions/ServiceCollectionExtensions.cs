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
            if (!environmentSettings.CurrencyConversionProvider.Equals("ExchangeRates", StringComparison.OrdinalIgnoreCase))
            {
                return services;
            }

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
