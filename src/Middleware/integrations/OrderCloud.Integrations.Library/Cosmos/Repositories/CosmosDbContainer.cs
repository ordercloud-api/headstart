using Microsoft.Azure.Cosmos;

namespace OrderCloud.Integrations.Library
{
    public interface ICosmosDbContainer
    {
        Container _container { get; }
    }

    public class CosmosDbContainer : ICosmosDbContainer
    {
        public CosmosDbContainer(
            CosmosClient cosmosClient,
            string databaseName,
            string containerName)
        {
            _container = cosmosClient.GetContainer(databaseName, containerName);
        }

        public Container _container { get; }
    }
}
