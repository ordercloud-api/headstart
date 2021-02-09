using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace ordercloud.integrations.library
{
    public static class OrderCloudWebHostBuilder
    {
        public static IWebHostBuilder CreateWebHostBuilder<TStartup, TAppSettings>(string[] args) where TStartup : class where TAppSettings : class, new() =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<TStartup>()
                .UseIISIntegration()

                .ConfigureServices((ctx, services) =>
                {
                    services.Configure<TAppSettings>(ctx.Configuration);
                    services.AddTransient(sp => sp.GetService<IOptionsSnapshot<TAppSettings>>().Value);
                });

        public static IWebHostBuilder CreateWebHostBuilder<TStartup, TAppSettings>(string[] args, string appSettingsConnectionString) where TStartup : class where TAppSettings : class, new() =>
            WebHost.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, config) =>
                {
                    config.AddAzureAppConfiguration(appSettingsConnectionString);
                })
                .UseStartup<TStartup>()
                .UseIISIntegration()

                .ConfigureServices((ctx, services) =>
                {
                    services.Configure<TAppSettings>(ctx.Configuration);
                    services.AddTransient(sp => sp.GetService<IOptionsSnapshot<TAppSettings>>().Value);
                });
    }
}