using Microsoft.Azure.Cosmos;

namespace ordercloud.integrations.library
{
    public interface IContainerContext<T> where T : CosmosObject
    {
        string ContainerName { get; }
        PartitionKey ResolvePartitionKey(string entityId);
    }
}
