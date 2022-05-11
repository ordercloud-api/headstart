using Headstart.Common.Models;
using Microsoft.Azure.Cosmos;
using ordercloud.integrations.library;

namespace Headstart.Common.Repositories
{
    public interface IPurchaseOrderDetailDataRepo : IRepository<OrderDetailData>
    {
    }
    public class PurchaseOrderDetailDataRepo : CosmosDbRepository<OrderDetailData>, IPurchaseOrderDetailDataRepo
    {
        public override string ContainerName { get; } = "purchaseorderdetail";
        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey("PartitionValue");
        public PurchaseOrderDetailDataRepo(ICosmosDbContainerFactory factory) : base(factory)
        { }
    }
}
