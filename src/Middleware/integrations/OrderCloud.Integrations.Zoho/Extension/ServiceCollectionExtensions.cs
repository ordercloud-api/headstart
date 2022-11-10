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

            if (string.IsNullOrEmpty(settings.AccessToken) ||
                string.IsNullOrEmpty(settings.ClientId) ||
                string.IsNullOrEmpty(settings.ClientSecret) ||
                string.IsNullOrEmpty(settings.OrganizationID))
            {
                throw new Exception("EnvironmentSettings:OMSProvider is set to 'Zoho' however missing required properties ZohoSettings:AccessToken, ZohoSettings:ClientId, ZohoSettings:ClientSecret, or ZohoSettings:OrganizationID. Please define these properties or set EnvironmentSettings:OMSProvider to an empty string to skip OMS integration");
            }

            services
                .AddSingleton<IZohoClient>(provider => new ZohoClient(settings, provider.GetService<IFlurlClientFactory>()))
                .AddSingleton<IOMSService, ZohoService>();

            return services;
        }
    }
}
