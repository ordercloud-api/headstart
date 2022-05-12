using Microsoft.Azure.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ordercloud.integrations.library
{
    public interface ICosmosDbContainerFactory
    {
        CosmosDbContainer GetContainer(string containerName);

        Task EnsureDbSetupAsync();
    }

    public class CosmosDbContainerFactory : ICosmosDbContainerFactory
    {
        private readonly CosmosClient cosmosClient;
        private readonly string databaseName;
        private readonly List<ContainerInfo> containers;

        public CosmosDbContainerFactory(
            CosmosClient cosmosClient,
            string databaseName,
            List<ContainerInfo> containers)
        {
            this.databaseName = databaseName ?? throw new ArgumentNullException(nameof(databaseName));
            this.containers = containers ?? throw new ArgumentNullException(nameof(containers));
            this.cosmosClient = cosmosClient ?? throw new ArgumentNullException(nameof(cosmosClient));
        }

        public CosmosDbContainer GetContainer(string containerName)
        {
            var exists = containers.Any(c => c.Name == containerName);
            if (!exists)
            {
                throw new ArgumentException($"Unable to find container: {containerName}");
            }

            return new CosmosDbContainer(cosmosClient, databaseName, containerName);
        }

        public async Task EnsureDbSetupAsync()
        {
            DatabaseResponse database = await cosmosClient.CreateDatabaseIfNotExistsAsync(databaseName);

            foreach (ContainerInfo container in containers)
            {
                await database.Database.CreateContainerIfNotExistsAsync(container.Name, $"{container.PartitionKey}");
            }
        }
    }
}
