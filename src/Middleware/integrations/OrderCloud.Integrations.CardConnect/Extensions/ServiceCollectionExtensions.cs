using System;
using Flurl.Http.Configuration;
using Headstart.Common.Services;
using Headstart.Common.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace OrderCloud.Integrations.CardConnect.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCardConnectCreditCartProcessor(this IServiceCollection services, EnvironmentSettings environmentSettings, CardConnectConfig cardConnectSettings)
        {
            if (!environmentSettings.PaymentProvider.Equals("CardConnect", StringComparison.OrdinalIgnoreCase))
            {
                return services;
            }

            services
                .AddSingleton(x => cardConnectSettings)
                .AddSingleton<ICardConnectClient>(provider =>
                    new CardConnectClient(cardConnectSettings, environmentSettings.Environment.ToString(), provider.GetService<IFlurlClientFactory>()))
                .AddSingleton<ICreditCardProcessor, CardConnectService>();

            return services;
        }
    }
}
