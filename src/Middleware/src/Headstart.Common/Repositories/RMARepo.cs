using Microsoft.Azure.Cosmos;
using ordercloud.integrations.library;
using Headstart.Common.Repositories.Models;

namespace Headstart.Common.Repositories
{
	public interface IRMARepo : IRepository<RMA>
	{
	}

	public class RMARepo : CosmosDbRepository<RMA>, IRMARepo
	{
		public override string ContainerName { get; } = @"rmas";
		public override PartitionKey ResolvePartitionKey(string entityId)
		{
			return new PartitionKey(@"PartitionValue");
		}

		public RMARepo(ICosmosDbContainerFactory factory) : base(factory)
		{
		}
	}
}