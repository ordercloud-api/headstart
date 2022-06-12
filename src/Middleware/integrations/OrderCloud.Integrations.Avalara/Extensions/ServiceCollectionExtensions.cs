using System;
using Avalara.AvaTax.RestClient;
using Headstart.Common.Services;
using Headstart.Common.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace OrderCloud.Integrations.Avalara.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddAvalaraTaxProvider(this IServiceCollection services, EnvironmentSettings environmentSettings, AvalaraConfig avalaraSettings)
        {
            if (!environmentSettings.TaxProvider.Equals("Avalara", StringComparison.OrdinalIgnoreCase))
            {
                return services;
            }

            var avaTaxClient = new AvaTaxClient("sitecore_headstart", "v1", "sitecore_headstart", new Uri(avalaraSettings.BaseApiUrl))
                .WithSecurity(avalaraSettings.AccountID, avalaraSettings.LicenseKey);
            var avalaraCommand = new AvalaraCommand(avaTaxClient, avalaraSettings, environmentSettings.Environment.ToString());

            services
                .AddSingleton<ITaxCodesProvider>(provider => avalaraCommand)
                .AddSingleton<ITaxCalculator>(provider => avalaraCommand);

            return services;
        }
    }
}
