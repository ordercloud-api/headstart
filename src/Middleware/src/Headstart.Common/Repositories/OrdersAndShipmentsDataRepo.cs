using Microsoft.Azure.Cosmos;
using ordercloud.integrations.library;
using Headstart.Common.Repositories.Models;

namespace Headstart.Common.Repositories
{
	public interface IOrdersAndShipmentsDataRepo : IRepository<OrderWithShipments>
	{
	}

	public class OrdersAndShipmentsDataRepo : CosmosDbRepository<OrderWithShipments>, IOrdersAndShipmentsDataRepo
	{
		public override string ContainerName { get; } = @"shipmentdetail";

		public override PartitionKey ResolvePartitionKey(string entityId)
		{
			return new PartitionKey(@"PartitionValue");
		}

		public OrdersAndShipmentsDataRepo(ICosmosDbContainerFactory factory) : base(factory)
		{
		}
	}
}