using Headstart.Common.Settings;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace OrderCloud.Integrations.Emails.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddDefaultEmailServiceProvider(this IServiceCollection services, EnvironmentSettings environmentSettings)
        {
            services.TryAddSingleton<IEmailServiceProvider, DefaultEmailServiceProvider>();

            return services;
        }
    }
}
