using Headstart.Common.Models;
using Microsoft.Azure.Cosmos;
using OrderCloud.Integrations.Library;

namespace Headstart.Common.Repositories
{
	public interface ILineItemDetailDataRepo : IRepository<LineItemDetailData>
	{
	}

	public class LineItemDetailDataRepo : CosmosDbRepository<LineItemDetailData>, ILineItemDetailDataRepo
	{
		public LineItemDetailDataRepo(ICosmosDbContainerFactory factory)
			: base(factory)
		{
		}

		public override string ContainerName { get; } = "lineitemdetail";

		public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey("PartitionValue");
	}
}
