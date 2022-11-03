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
        public static IServiceCollection AddEasyPostShippingProvider(this IServiceCollection services, EnvironmentSettings environmentSettings, EasyPostSettings easyPostSettings) {
            if (!environmentSettings.ShippingProvider.Equals("EasyPost", StringComparison.OrdinalIgnoreCase) || string.IsNullOrEmpty(easyPostSettings.ApiKey)) {
                return services;
            }

            if (string.IsNullOrEmpty(easyPostSettings.CustomsSigner))
            {
                Console.WriteLine("Unable to use EasyPost as a shipping provider as required property EasyPostSettings:CustomSigner was not provided");
                return services;
            }

            if (string.IsNullOrEmpty(easyPostSettings.USPSAccountId) || string.IsNullOrEmpty(easyPostSettings.FedexAccountId))
            {
                Console.WriteLine("Unable to use EasyPost as a shipping provider as neither UpsAccountId nor FedexAccountId were provided");
                return services;
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
