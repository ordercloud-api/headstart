using Microsoft.Azure.Cosmos;
using ordercloud.integrations.library;
using Headstart.Common.Repositories.Models;

namespace Headstart.Common.Repositories
{
	public interface ISalesOrderDetailDataRepo : IRepository<OrderDetailData>
	{
	}

	public class SalesOrderDetailDataRepo : CosmosDbRepository<OrderDetailData>, ISalesOrderDetailDataRepo
	{
		public override string ContainerName { get; } = @"salesorderdetail";
		public override PartitionKey ResolvePartitionKey(string entityId)
		{
			return new PartitionKey(@"PartitionValue");
		}

		public SalesOrderDetailDataRepo(ICosmosDbContainerFactory factory) : base(factory)
		{
		}
	}
}