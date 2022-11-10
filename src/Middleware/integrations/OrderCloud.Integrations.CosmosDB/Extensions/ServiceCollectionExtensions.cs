using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Cosmonaut;
using Cosmonaut.Extensions.Microsoft.DependencyInjection;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Documents.Client;
using Microsoft.Extensions.DependencyInjection;

namespace OrderCloud.Integrations.CosmosDB.Extensions
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection InjectCosmosStore<TQuery, TModel>(this IServiceCollection services, CosmosConfig config)
            where TQuery : class
            where TModel : class
        {
            if (config.DatabaseName == null || config.EndpointUri == null || config.PrimaryKey == null)
            {
                // allow server to be started up without these settings
                // in case they're just trying to seed their environment
                // in the future we'll remove this in favor of centralized seeding capability
                return services;
            }

            var settings = new CosmosStoreSettings(
                config.DatabaseName,
                config.EndpointUri,
                config.PrimaryKey,
                new ConnectionPolicy
                {
                    ConnectionProtocol = Protocol.Tcp,
                    ConnectionMode = Microsoft.Azure.Documents.Client.ConnectionMode.Direct,
                    RequestTimeout = config.RequestTimeout,
                },
                defaultCollectionThroughput: 400)
            {
                UniqueKeyPolicy = new Microsoft.Azure.Documents.UniqueKeyPolicy()
                {
                    UniqueKeys =
                        (Collection<Microsoft.Azure.Documents.UniqueKey>)typeof(TModel).GetMethod("GetUniqueKeys")?.Invoke(null, null) ??
                        new Collection<Microsoft.Azure.Documents.UniqueKey>(),
                },
            };
            services.AddSingleton(typeof(TQuery), typeof(TQuery));
            return services.AddCosmosStore<TModel>(settings);
        }

        public static IServiceCollection AddCosmosDb(
            this IServiceCollection services,
            CosmosSettings settings,
            List<ContainerInfo> containers)
        {
            if (settings.EndpointUri == null || settings.PrimaryKey == null || settings.DatabaseName == null)
            {
                throw new Exception("Required properties CosmosSettings:EndpointUri, CosmosSettings:PrimaryKey, or CosmosSettings:DatabaseName are missing");
            }

            CosmosClient client = new CosmosClient(settings.EndpointUri, settings.PrimaryKey);
            var cosmosDbClientFactory = new CosmosDbContainerFactory(client, settings.DatabaseName, containers);

            services.AddSingleton<ICosmosDbContainerFactory>(cosmosDbClientFactory);

            return services;
        }
    }
}
