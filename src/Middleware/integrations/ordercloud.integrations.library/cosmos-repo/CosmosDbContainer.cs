using Microsoft.Azure.Cosmos;

namespace ordercloud.integrations.library
{
    public interface ICosmosDbContainer
    {
        Container _container { get; }
    }
    public class CosmosDbContainer : ICosmosDbContainer
    {
        public Container _container { get; }

        public CosmosDbContainer(CosmosClient cosmosClient,
                                 string databaseName,
                                 string containerName)
        {
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }
    }
}
