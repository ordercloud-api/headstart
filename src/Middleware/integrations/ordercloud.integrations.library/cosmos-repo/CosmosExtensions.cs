using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Azure.Cosmos;

namespace ordercloud.integrations.library
{
    public static class IServiceCollectionExtensions
    {
        public static IServiceCollection AddCosmosDb(this IServiceCollection services,
                                                     string endpointUrl,
                                                     string primaryKey,
                                                     string databaseName,
                                                     List<ContainerInfo> containers)
        {
            if(endpointUrl == null || primaryKey == null || databaseName == null)
            {
                // allow server to be started up without these settings
                // in case they're just trying to seed their environment
                // in the future we'll remove this in favor of centralized seeding capability
                return services;
            }
            CosmosClient client = new CosmosClient(endpointUrl, primaryKey);
            var cosmosDbClientFactory = new CosmosDbContainerFactory(client, databaseName, containers);

            services.AddSingleton<ICosmosDbContainerFactory>(cosmosDbClientFactory);

            return services;
        }

    }
}