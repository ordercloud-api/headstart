using System;
using Headstart.Common.Services;
using Headstart.Common.Settings;
using Microsoft.Extensions.DependencyInjection;
using Taxjar;

namespace OrderCloud.Integrations.TaxJar.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddTaxJarTaxProvider(this IServiceCollection services, EnvironmentSettings environmentSettings, TaxJarConfig taxJarSettings)
        {
            if (!environmentSettings.TaxProvider.Equals("TaxJar", StringComparison.OrdinalIgnoreCase))
            {
                return services;
            }

            if (string.IsNullOrEmpty(taxJarSettings.ApiKey))
            {
                throw new Exception("EnvironmentSettings:TaxProvider is set to 'TaxJar' however missing required property TaxJarSettings:ApiKey. Please define this property or set EnvironmentSettings:TaxProvider to an empty string to use mocked tax rates");
            }

            var apiUrl = taxJarSettings.Environment == TaxJarEnvironment.Production ? TaxjarConstants.DefaultApiUrl : TaxjarConstants.SandboxApiUrl;
            var taxjarClient = new TaxjarApi(taxJarSettings.ApiKey, new { apiUrl });

            var taxJarCommand = new TaxJarCommand(taxjarClient);

            services
                .AddSingleton<ITaxCodesProvider>(provider => taxJarCommand)
                .AddSingleton<ITaxCalculator>(provider => taxJarCommand);

            return services;
        }
    }
}
