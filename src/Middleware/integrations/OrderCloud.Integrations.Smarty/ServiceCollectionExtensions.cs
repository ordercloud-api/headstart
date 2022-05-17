using Microsoft.Extensions.DependencyInjection;
using OrderCloud.SDK;
using SmartyStreets;

namespace OrderCloud.Integrations.Smarty
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSmartyIntegration(this IServiceCollection services)
        {
            var settings = new SmartyStreetsConfig
            {
                SmartyEnabled = true,
                AuthID = "cfcd0554-714e-cd21-216d-8fd389fadbd8",
                AuthToken = "vh6iJbBG7JEKMqfR3pdN",
                RefererHost = string.Empty,
                WebsiteKey = string.Empty,
            };

            var smartyStreetsUsClient = new ClientBuilder(settings.AuthID, settings.AuthToken).BuildUsStreetApiClient();

            var smartyService = new SmartyStreetsService(settings, smartyStreetsUsClient);

            var orderCloudClient = new OrderCloudClient(new OrderCloudClientConfig
            {
                ApiUrl = "https://australiaeast-sandbox.ordercloud.io",
                AuthUrl = "https://australiaeast-sandbox.ordercloud.io",
                ClientId = "2211D27D-6A75-4F31-915F-77C735C6448F",
                ClientSecret = "3hHPsqUKxMKXzLYbrsWYfD7xLm6IHou8kg3B0m3JZ0wWLaAesOJ8jWf0jSEG",
                Roles = new[] { ApiRole.FullAccess },
            });

            services
                .AddSingleton<ISmartyStreetsCommand>(x => new SmartyStreetsCommand(settings, orderCloudClient, smartyService))
                .AddSingleton<ISmartyStreetsService>(x => smartyService);

            return services;
        }
    }
}
