using System;
using Flurl.Http.Configuration;
using Headstart.Common.Services;
using Headstart.Common.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace OrderCloud.Integrations.Zoho.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddZohoOMSProvider(this IServiceCollection services, EnvironmentSettings environmentSettings, ZohoConfig settings)
        {
            if (!environmentSettings.OMSProvider.Equals("Zoho", StringComparison.OrdinalIgnoreCase))
            {
                return services;
            }

            services
                .AddSingleton<IZohoClient>(provider => new ZohoClient(settings, provider.GetService<IFlurlClientFactory>()))
                .AddSingleton<IOMSService, ZohoService>();

            return services;
        }
    }
}
