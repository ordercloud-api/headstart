using System.Collections.ObjectModel;
using Cosmonaut;
using Cosmonaut.Extensions.Microsoft.DependencyInjection;
using Microsoft.Azure.Documents;
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
                    ConnectionMode = ConnectionMode.Direct,
                    RequestTimeout = config.RequestTimeout,
                },
                defaultCollectionThroughput: 400)
            {
                UniqueKeyPolicy = new UniqueKeyPolicy()
                {
                    UniqueKeys =
                        (Collection<UniqueKey>)typeof(TModel).GetMethod("GetUniqueKeys")?.Invoke(null, null) ??
                        new Collection<UniqueKey>(),
                },
            };
            services.AddSingleton(typeof(TQuery), typeof(TQuery));
            return services.AddCosmosStore<TModel>(settings);
        }
    }
}
