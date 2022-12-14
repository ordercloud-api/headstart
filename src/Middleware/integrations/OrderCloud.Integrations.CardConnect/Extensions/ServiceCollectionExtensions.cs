using System;
using Flurl.Http.Configuration;
using Headstart.Common.Services;
using Headstart.Common.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace OrderCloud.Integrations.CardConnect.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddCardConnectCreditCartProcessor(this IServiceCollection services, EnvironmentSettings environmentSettings, CardConnectSettings cardConnectSettings)
        {
            if (!environmentSettings.PaymentProvider.Equals("CardConnect", StringComparison.OrdinalIgnoreCase))
            {
                return services;
            }

            if (string.IsNullOrEmpty(cardConnectSettings.Authorization) || string.IsNullOrEmpty(cardConnectSettings.MerchantID))
            {
                throw new Exception("EnvironmentSettings:PaymentProvider is set to 'CardConnect' however missing required properties CardConnectSettings:Authorization or CardConnectSettings:MerchantID. Please define these properties or set EnvironmentSettings:PaymentProvider to an empty string to use mocked credit card payments");
            }

            services
                .AddSingleton(x => cardConnectSettings)
                .AddSingleton<ICardConnectClient>(provider =>
                    new CardConnectClient(cardConnectSettings, environmentSettings.Environment.ToString(), provider.GetService<IFlurlClientFactory>()))
                .AddSingleton<IHSCreditCardProcessor, CardConnectService>();

            return services;
        }
    }
}
