using Microsoft.Azure.Cosmos;
using Headstart.Common.Models;
using ordercloud.integrations.library;

namespace Headstart.Common.Repositories
{
    public interface IOrdersAndShipmentsDataRepo : IRepository<OrderWithShipments> { }

    public class OrdersAndShipmentsDataRepo : CosmosDbRepository<OrderWithShipments>, IOrdersAndShipmentsDataRepo
    {
        public override string ContainerName { get; } = $@"shipmentdetail";

        public override PartitionKey ResolvePartitionKey(string entityId)
        {
            return new PartitionKey($@"PartitionValue");
        }

        public OrdersAndShipmentsDataRepo(ICosmosDbContainerFactory factory) : base(factory) { }
    }
}