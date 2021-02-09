
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Headstart.Common.Helpers;
using Microsoft.AspNetCore.Authentication;
using System;
using Microsoft.Extensions.DependencyInjection;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace ordercloud.integrations.library
{
    public static class OrderCloudIntegrationsConfigureWebApiServicesExtensions
    {
        public static IServiceCollection OrderCloudIntegrationsConfigureWebApiServices<T>(this IServiceCollection services, T settings, BlobServiceConfig errorLogBlobConfig, string corsPolicyName = null)
            where T : class
        {
            services.AddSingleton(x => new GlobalExceptionHandler(errorLogBlobConfig));
            services.Inject<IOrderCloudClient>();
            services.AddSingleton(settings);
            services
                .AddCors(o =>
                    o.AddPolicy(
                        name: corsPolicyName ?? Environment.GetEnvironmentVariable("CORS_POLICY"),
                        builder => {
                            builder
                                .AllowAnyOrigin()
                                .AllowAnyMethod()
                                .AllowAnyHeader();
                        }));
            services
                .AddControllers(o => { o.Filters.Add(typeof(ValidateModelAttribute)); })
                .AddNewtonsoftJson(o => {
                    o.SerializerSettings.ContractResolver = new HSSerializer();
                    o.SerializerSettings.Converters.Add(new StringEnumConverter());
                });
            return services;
        }

        public static IServiceCollection AddAuthenticationScheme<TAuthOptions, TAuthHandler>(this IServiceCollection services, string name)
            where TAuthOptions : AuthenticationSchemeOptions, new() where TAuthHandler : AuthenticationHandler<TAuthOptions>
        {
            services.AddAuthentication().AddScheme<TAuthOptions, TAuthHandler>(name, null);
            return services;
        }

        public static IServiceCollection AddAuthenticationScheme<TAuthOptions, TAuthHandler>(this IServiceCollection services, string name, Action<TAuthOptions> configureOptions)
            where TAuthOptions : AuthenticationSchemeOptions, new() where TAuthHandler : AuthenticationHandler<TAuthOptions>
        {
            services.AddAuthentication().AddScheme<TAuthOptions, TAuthHandler>(name, null, configureOptions);
            return services;
        }
    }
}