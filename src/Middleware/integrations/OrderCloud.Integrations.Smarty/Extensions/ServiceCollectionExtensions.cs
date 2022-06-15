using System;
using Headstart.Common.Commands;
using Headstart.Common.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace OrderCloud.Integrations.Smarty.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSmartyAddressValidationProvider(this IServiceCollection services, EnvironmentSettings environmentSettings, SmartyStreetsConfig smartySettings)
        {
            if (!environmentSettings.AddressValidationProvider.Equals("Smarty", StringComparison.OrdinalIgnoreCase))
            {
                return services;
            }

            services
                .AddSingleton(x => smartySettings)
                .AddSingleton<ISmartyStreetsService, SmartyStreetsService>()
                .AddSingleton<IAddressCommand, SmartyStreetsCommand>()
                .AddSingleton<IAddressValidationCommand, SmartyStreetsCommand>();

            return services;
        }
    }
}
