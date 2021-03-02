
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json.Converters;
using Headstart.Common.Helpers;
using Microsoft.AspNetCore.Authentication;
using System;
using Microsoft.Extensions.DependencyInjection;
using OrderCloud.SDK;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.AspNetCore.Builder;

namespace ordercloud.integrations.library
{
    public static class OrderCloudIntegrationsConfigureWebApiServicesExtensions
    {
        public static IServiceCollection OrderCloudIntegrationsConfigureWebApiServices<T>(this IServiceCollection services, T settings, BlobServiceConfig errorLogBlobConfig, string corsPolicyName = null)
            where T : class
        {
            // false became the default in asp.net core 3.0 to combat application hangs
            // however we're using synchronous APIs when validating webhook hash
            // specifically ComputeHash will trigger an error here
            // TODO: figure out how to compute the hash in an async manner so we can remove this
            services.Configure<KestrelServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
            services.Configure<IISServerOptions>(options =>
            {
                options.AllowSynchronousIO = true;
            });
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