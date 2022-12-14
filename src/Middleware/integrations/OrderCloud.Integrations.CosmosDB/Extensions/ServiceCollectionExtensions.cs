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
            if (string.IsNullOrEmpty(config.DatabaseName) || string.IsNullOrEmpty(config.EndpointUri) || string.IsNullOrEmpty(config.PrimaryKey))
            {
                // CosmosDB is used to store data that doesn't belong in OrderCloud but is required for a complete solution, one example is reporting
                throw new Exception($"Please provide the required app settings: CosmosSettings:DatabaseName, CosmosSettings:EndpointUri, and CosmosSettings:PrimaryKey");
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
            if (string.IsNullOrEmpty(settings.DatabaseName) || string.IsNullOrEmpty(settings.EndpointUri) || string.IsNullOrEmpty(settings.PrimaryKey))
            {
                // CosmosDB is used to store data that doesn't belong in OrderCloud but is required for a complete solution, one example is reporting
                throw new Exception("Please provide the required app settings: CosmosSettings:EndpointUri, CosmosSettings:PrimaryKey, or CosmosSettings:DatabaseName");
            }

            CosmosClient client = new CosmosClient(settings.EndpointUri, settings.PrimaryKey);
            var cosmosDbClientFactory = new CosmosDbContainerFactory(client, settings.DatabaseName, containers);

            services.AddSingleton<ICosmosDbContainerFactory>(cosmosDbClientFactory);

            return services;
        }
    }
}
