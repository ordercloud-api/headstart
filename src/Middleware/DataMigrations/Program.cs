#define TRACE
using System;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DataMigrations.Migrations;
using Headstart.Common;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace DataMigrations
{
	public class Program
	{
		private static ServiceProvider _provider;

		static async Task Main(string[] args)
		{
			var services = new ServiceCollection();
			var builder = new ConfigurationBuilder();
			var settings = new AppSettings();

			builder.AddAzureAppConfiguration(Environment.GetEnvironmentVariable("APP_CONFIG_CONNECTION"));
			var config = builder.Build();
			config.Bind(settings);

			var cosmosConfig = new CosmosConfig(
				settings.CosmosSettings.DatabaseName,
				settings.CosmosSettings.EndpointUri,
				settings.CosmosSettings.PrimaryKey,
				settings.CosmosSettings.RequestTimeoutInSeconds,
				settings.CosmosSettings.MaxConnectionLimit,
				settings.CosmosSettings.IdleTcpConnectionTimeoutInMinutes,
				settings.CosmosSettings.OpenTcpConnectionTimeoutInSeconds,
				settings.CosmosSettings.MaxTcpConnectionsPerEndpoint,
				settings.CosmosSettings.MaxRequestsPerTcpConnection,
				settings.CosmosSettings.EnableTcpConnectionEndpointRediscovery
			);

			_provider = services
				.AddSingleton(settings)
				.AddSingleton<ICosmosBulkOperations>(new CosmosBulkOperations(cosmosConfig))
				.BuildServiceProvider();

			Trace.AutoFlush = true;
			Trace.Listeners.Add(new TextWriterTraceListener(Console.Out));
			Trace.WriteLine("Entering Main");

			var editor = _provider.GetService<ICosmosBulkOperations>();

			var migration = new one_big_bucket_option_13oct2020(editor);

			await migration.Run();
		}
	}
}
