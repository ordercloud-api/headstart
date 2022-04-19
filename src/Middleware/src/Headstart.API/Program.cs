using System;
using Headstart.Common;
using Microsoft.AspNetCore;
using Microsoft.Extensions.Options;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Headstart.API
{
	public static class Program
	{
		/// <summary>
		/// The Program.Main void method
		/// </summary>
		/// <param name="args"></param>
		public static void Main(string[] args)
		{
			// Links to an Azure App Configuration resource that holds the app settings.
			// Set this in your visual studio Env Variables.
			var appConfigConnectionString = Environment.GetEnvironmentVariable(@"APP_CONFIG_CONNECTION");
			WebHost.CreateDefaultBuilder(args).UseDefaultServiceProvider(options => options.ValidateScopes = false)
				.ConfigureAppConfiguration((context, config) =>
				{
					if (!string.IsNullOrEmpty(appConfigConnectionString))
					{
						config.AddAzureAppConfiguration(appConfigConnectionString);
					}
					config.AddJsonFile(@"appSettings.json", optional: true);
				}).UseStartup<Startup>()
				.ConfigureServices((ctx, services) =>
				{
					services.Configure<AppSettings>(ctx.Configuration);
					services.AddTransient(sp => sp.GetService<IOptionsSnapshot<AppSettings>>().Value);
				}).Build().Run();
		}
	}
}