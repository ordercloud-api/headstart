using Microsoft.Azure.Cosmos;
using OrderCloud.Integrations.Library.Cosmos;

namespace OrderCloud.Integrations.Library
{
    public interface IContainerContext<T> where T : CosmosObject
    {
        string ContainerName { get; }

        PartitionKey ResolvePartitionKey(string entityId);
    }
}
