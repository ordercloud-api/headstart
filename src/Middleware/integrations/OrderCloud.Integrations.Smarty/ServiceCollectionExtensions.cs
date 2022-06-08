using Headstart.Common.Commands;
using Microsoft.Extensions.DependencyInjection;
using OrderCloud.SDK;
using SmartyStreets;

namespace OrderCloud.Integrations.Smarty
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSmartyIntegration(this IServiceCollection services, SmartyStreetsConfig settings, IOrderCloudClient orderCloudClient)
        {
            if (!settings.SmartyEnabled)
            {
                return services;
            }

            var smartyStreetsUsClient = new ClientBuilder(settings.AuthID, settings.AuthToken).BuildUsStreetApiClient();
            var smartyService = new SmartyStreetsService(settings, smartyStreetsUsClient);

            services
                .AddSingleton<IAddressValidationCommand>(x => new SmartyStreetsCommand(settings, orderCloudClient, smartyService))
                .AddSingleton<ISmartyStreetsService>(x => smartyService);

            return services;
        }
    }
}
