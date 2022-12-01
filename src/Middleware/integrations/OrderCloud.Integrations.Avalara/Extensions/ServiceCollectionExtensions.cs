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

            if (string.IsNullOrEmpty(avalaraSettings.BaseApiUrl) ||
                avalaraSettings.AccountID == 0 ||
                string.IsNullOrEmpty(avalaraSettings.LicenseKey) ||
                avalaraSettings.CompanyID == 0)
            {
                throw new Exception("EnvironmentSettings:TaxProvider is set to 'Avalara' however missing required properties AvalaraSettings:BaseApiUrl, AvalaraSettings:AccountID, AvalaraSettings:LicenseKey, or AvalaraSettings:CompanyID. Please define these properties or set EnvironmentSettings:TaxProvider to an empty string to use mocked tax rates");
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
