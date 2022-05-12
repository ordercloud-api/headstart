using Headstart.Common.Models;
using Microsoft.Azure.Cosmos;
using ordercloud.integrations.library;

namespace Headstart.Common.Repositories
{
    public interface IRMARepo : IRepository<RMA>
    {
    }
    public class RMARepo : CosmosDbRepository<RMA>, IRMARepo
    {
        public override string ContainerName { get; } = "rmas";
        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey("PartitionValue");
        public RMARepo(ICosmosDbContainerFactory factory) : base(factory)
        { }
    }
}
