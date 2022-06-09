using Microsoft.Azure.Cosmos;
using OrderCloud.Integrations.CosmosDB;
using OrderCloud.Integrations.Reporting.Models;

namespace OrderCloud.Integrations.Reporting.Repositories
{
    public interface IOrdersAndShipmentsDataRepo : IRepository<OrderWithShipments>
    {
    }

    public class OrdersAndShipmentsDataRepo : CosmosDbRepository<OrderWithShipments>, IOrdersAndShipmentsDataRepo
    {
        public OrdersAndShipmentsDataRepo(ICosmosDbContainerFactory factory)
            : base(factory)
        {
        }

        public override string ContainerName { get; } = "shipmentdetail";

        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey("PartitionValue");
    }
}
