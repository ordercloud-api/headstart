using Microsoft.Azure.Cosmos;
using Headstart.Common.Models;
using ordercloud.integrations.library;

namespace Headstart.Common.Repositories
{
    public interface IRMARepo : IRepository<RMA> { }

    public class RMARepo : CosmosDbRepository<RMA>, IRMARepo
    {
        public override string ContainerName { get; } = $@"rmas";
        public override PartitionKey ResolvePartitionKey(string entityId)
        {
            return new PartitionKey($@"PartitionValue");
        }

        public RMARepo(ICosmosDbContainerFactory factory) : base(factory) { }
    }
}