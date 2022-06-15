using System;
using Headstart.Common.Settings;
using Microsoft.Extensions.DependencyInjection;
using OrderCloud.Integrations.Emails;
using SendGrid;

namespace OrderCloud.Integrations.SendGrid.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddSendGridEmailServiceProvider(this IServiceCollection services, EnvironmentSettings environmentSettings, SendGridSettings sendGridSettings, UI uiSettings)
        {
            if (!environmentSettings.EmailServiceProvider.Equals("SendGrid", StringComparison.OrdinalIgnoreCase))
            {
                return services;
            }

            services
                .AddSingleton(x => sendGridSettings)
                .AddSingleton(x => uiSettings)
                .AddSingleton<ISendGridClient>(x => new SendGridClient(sendGridSettings.ApiKey))
                .AddSingleton<IEmailServiceProvider, SendGridService>();

            return services;
        }
    }
}
