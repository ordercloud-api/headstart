using Microsoft.Azure.Cosmos;
using OrderCloud.Integrations.CosmosDB;
using OrderCloud.Integrations.Reporting.Models;

namespace OrderCloud.Integrations.Reporting.Repositories
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
