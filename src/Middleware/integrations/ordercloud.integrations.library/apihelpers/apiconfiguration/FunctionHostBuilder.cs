using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.Json.Serialization;
using Cosmonaut;
using Cosmonaut.Extensions.Microsoft.DependencyInjection;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Configuration;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Headstart.Common.Helpers;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;
using Newtonsoft.Json;

namespace ordercloud.integrations.library
{
    public static class FunctionHostBuilderExtensions
    {
        public static T BindSettings<T>(
            this IFunctionsHostBuilder host, 
            string connection_string = null,
            string section = "AppSettings"
         )
            where T : class, new()
        {
            var settings = new T();

            var config = new ConfigurationBuilder()
                .AddEnvironmentVariables()
                .AddAzureAppConfiguration(connection_string ?? Environment.GetEnvironmentVariable("APP_CONFIG_CONNECTION"))
                .Build();

            config
                .GetSection(section ?? Environment.GetEnvironmentVariable("APP_SETTINGS_SECTION_NAME"))
                .Bind(settings);

            host
                .Services.AddMvcCore()
                .AddNewtonsoftJson(o => {
                    o.SerializerSettings.ContractResolver = new DefaultContractResolver();
                    o.SerializerSettings.Converters.Add(new StringEnumConverter());
                    o.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                })
                .Services.Replace(ServiceDescriptor.Singleton(typeof(IConfiguration), config))
                .BuildServiceProvider()
                .GetService<IConfiguration>();

            host.Services.AddSingleton(settings);
            return settings;
        }

        public static IFunctionsHostBuilder InjectDependencies(this IFunctionsHostBuilder builder, IEnumerable<Type> services)
        {
            foreach (var service in services)
            {
                builder.Services.Inject(service);
            }

            return builder;
        }

        public static IHostBuilder InjectCosmosStores<TQuery, TModel>(this IHostBuilder builder, CosmosConfig config) where TQuery : class where TModel : class
        {
            return builder.ConfigureServices((context, collection) =>
            {
                var cs = new CosmosStoreSettings(config.DatabaseName, config.EndpointUri, config.PrimaryKey, new ConnectionPolicy
                {
                    ConnectionProtocol = Protocol.Tcp,
                    ConnectionMode = ConnectionMode.Direct,
                    RequestTimeout = config.RequestTimeout

                }, defaultCollectionThroughput: 400)
                {
                    UniqueKeyPolicy = new UniqueKeyPolicy()
                    {
                        UniqueKeys = (Collection<UniqueKey>)typeof(TModel).GetMethod("GetUniqueKeys")?.Invoke(null, null) ?? new Collection<UniqueKey>()
                    }
                };
                collection.AddSingleton(typeof(TQuery), typeof(TQuery));
                collection.AddCosmosStore<TModel>(cs);
            });
        }
    }
}
