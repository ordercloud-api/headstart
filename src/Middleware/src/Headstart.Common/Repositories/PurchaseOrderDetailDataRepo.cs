using Headstart.Common.Models;
using Microsoft.Azure.Cosmos;
using OrderCloud.Integrations.Library;

namespace Headstart.Common.Repositories
{
    public interface IPurchaseOrderDetailDataRepo : IRepository<OrderDetailData>
    {
    }

    public class PurchaseOrderDetailDataRepo : CosmosDbRepository<OrderDetailData>, IPurchaseOrderDetailDataRepo
    {
        public PurchaseOrderDetailDataRepo(ICosmosDbContainerFactory factory)
            : base(factory)
        {
        }

        public override string ContainerName { get; } = "purchaseorderdetail";

        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey("PartitionValue");
    }
}
