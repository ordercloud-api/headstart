using System;
using Headstart.Common;
using Microsoft.AspNetCore.Hosting;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;

namespace Headstart.API
{
	public static class Program
	{
		public static void Main(string[] args)
		{
			// Links to an Azure App Configuration resource that holds the app settings.
			// Set this in your visual studio Env Variables.
			var connectionString = Environment.GetEnvironmentVariable("APP_CONFIG_CONNECTION"); 

			CatalystWebHostBuilder
				.CreateWebHostBuilder<Startup, AppSettings>(args, connectionString)
				.Build()
				.Run();
		}
	}
}
