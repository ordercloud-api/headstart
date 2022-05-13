using Headstart.Common.Models;
using Microsoft.Azure.Cosmos;
using OrderCloud.Integrations.Library;

namespace Headstart.Common.Repositories
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
