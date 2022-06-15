using Headstart.Common.Extensions;
using Headstart.Common.Settings;
using Microsoft.Extensions.DependencyInjection;
using OrderCloud.Integrations.RMAs.Commands;
using OrderCloud.Integrations.RMAs.Repositories;

namespace OrderCloud.Integrations.RMAs.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultRMAsProvider(this IServiceCollection services, EnvironmentSettings environmentSettings)
        {
            services
                .AddSingleton<IRMACommand, RMACommand>()
                .Inject<IRMARepo>();

            return services;
        }
    }
}
