using System;
using Headstart.Common.Commands;
using Headstart.Common.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace OrderCloud.Integrations.Smarty.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSmartyAddressValidationProvider(this IServiceCollection services, EnvironmentSettings environmentSettings, SmartyStreetSettings smartySettings)
        {
            if (!environmentSettings.AddressValidationProvider.Equals("Smarty", StringComparison.OrdinalIgnoreCase))
            {
                return services;
            }

            if (string.IsNullOrEmpty(smartySettings.AuthID) || string.IsNullOrEmpty(smartySettings.AuthToken))
            {
                throw new Exception("EnvironmentSettings:AddressValidationProvider is set to 'Smarty' however missing required properties SmartyStreetSettings:AuthID or SmartyStreetSettings:AuthToken. Please define these properties or set EnvironmentSettings:AddressValidationProvider to an empty string to skip address validation");
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
