using Microsoft.Azure.Cosmos;
using ordercloud.integrations.library;
using Headstart.Common.Repositories.Models;

namespace Headstart.Common.Repositories
{
	public interface IPurchaseOrderDetailDataRepo : IRepository<OrderDetailData>
	{
	}

	public class PurchaseOrderDetailDataRepo : CosmosDbRepository<OrderDetailData>, IPurchaseOrderDetailDataRepo
	{
		public override string ContainerName { get; } = @"purchaseorderdetail";
		public override PartitionKey ResolvePartitionKey(string entityId)
		{
			return new PartitionKey(@"PartitionValue");
		}

		public PurchaseOrderDetailDataRepo(ICosmosDbContainerFactory factory) : base(factory)
		{
		}
	}
}