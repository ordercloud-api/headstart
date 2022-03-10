using Microsoft.Azure.Cosmos;
using ordercloud.integrations.library;
using Headstart.Common.Repositories.Models;

namespace Headstart.Common.Repositories
{
	public interface ILineItemDetailDataRepo : IRepository<LineItemDetailData> { }

	public class LineItemDetailDataRepo : CosmosDbRepository<LineItemDetailData>, ILineItemDetailDataRepo
	{
		public override string ContainerName { get; } = @"lineitemdetail";

		public override PartitionKey ResolvePartitionKey(string entityId)
		{
			return new PartitionKey(@"PartitionValue");
		}

		public LineItemDetailDataRepo(ICosmosDbContainerFactory factory) : base(factory)
		{
		}
	}
}