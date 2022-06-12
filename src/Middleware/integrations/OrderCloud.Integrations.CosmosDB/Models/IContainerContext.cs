using Microsoft.Azure.Cosmos;

namespace OrderCloud.Integrations.CosmosDB
{
    public interface IContainerContext<T> where T : CosmosObject
    {
        string ContainerName { get; }

        PartitionKey ResolvePartitionKey(string entityId);
    }
}
