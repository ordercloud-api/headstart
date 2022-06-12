using System;
using Headstart.Common.Commands;
using Headstart.Common.Services;
using Headstart.Common.Settings;
using Microsoft.Extensions.DependencyInjection;
using OrderCloud.Integrations.EasyPost.Commands;

namespace OrderCloud.Integrations.EasyPost.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddEasyPostShippingProvider(this IServiceCollection services, EnvironmentSettings environmentSettings, EasyPostSettings easyPostSettings)
        {
            if (!environmentSettings.ShippingProvider.Equals("EasyPost", StringComparison.OrdinalIgnoreCase))
            {
                return services;
            }

            services
                .AddSingleton(x => easyPostSettings)
                .AddSingleton(x => new EasyPostClient(easyPostSettings.ApiKey))
                .AddSingleton<IShippingCommand, EasyPostShippingCommand>()
                .AddSingleton<IShippingService, EasyPostShippingService>();

            return services;
        }
    }
}
