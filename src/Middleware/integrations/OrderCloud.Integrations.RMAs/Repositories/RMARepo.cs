using Microsoft.Azure.Cosmos;
using OrderCloud.Integrations.Library;
using OrderCloud.Integrations.RMAs.Models;

namespace OrderCloud.Integrations.RMAs.Repositories
{
    public interface IRMARepo : IRepository<RMA>
    {
    }

    public class RMARepo : CosmosDbRepository<RMA>, IRMARepo
    {
        public RMARepo(ICosmosDbContainerFactory factory)
            : base(factory)
        {
        }

        public override string ContainerName { get; } = "rmas";

        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey("PartitionValue");
    }
}
