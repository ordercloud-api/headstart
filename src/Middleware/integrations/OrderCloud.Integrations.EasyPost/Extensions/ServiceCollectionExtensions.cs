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
            if (!environmentSettings.ShippingProvider.Equals("EasyPost", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(easyPostSettings.ApiKey))
            {
                return services;
            }

            if (string.IsNullOrWhiteSpace(easyPostSettings.ApiKey) || string.IsNullOrEmpty(easyPostSettings.CustomsSigner))
            {
                throw new Exception("EnvironmentSettings:ShippingProvider is set to 'EasyPost' however missing required properties EasyPostSettings:ApiKey or EasyPostSettings:CustomsSigner. Please define these properties or set EnvironmentSettings:ShippingProvider to an empty string to use mocked shipping rates");
            }

            if (string.IsNullOrEmpty(easyPostSettings.USPSAccountId) && string.IsNullOrEmpty(easyPostSettings.FedexAccountId))
            {
                throw new Exception("EnvironmentSettings:ShippingProvider is set to 'EasyPost' however at least one of EasyPostSettings:USPSAccountId or EasyPostSettingsFedexAccountId must be defined. Please define or set EnvironmentSettings:ShippingProvider to an empty string to use mocked shipping rates");
            }

            services
                .AddSingleton(x => easyPostSettings)
                .AddSingleton(x => new EasyPostClient(easyPostSettings.ApiKey))
                .AddSingleton<IShippingCommand, EasyPostShippingCommand>()
                .AddSingleton<IShippingService, EasyPostShippingService>()
                .AddSingleton<IEasyPostShippingService, EasyPostShippingService>();

            return services;
        }
    }
}
