using Microsoft.Azure.Cosmos;
using OrderCloud.Integrations.CosmosDB;
using OrderCloud.Integrations.Reporting.Models;

namespace OrderCloud.Integrations.Reporting.Repositories
{
    public interface ISalesOrderDetailDataRepo : IRepository<OrderDetailData>
    {
    }

    public class SalesOrderDetailDataRepo : CosmosDbRepository<OrderDetailData>, ISalesOrderDetailDataRepo
    {
        public SalesOrderDetailDataRepo(ICosmosDbContainerFactory factory)
            : base(factory)
        {
        }

        public override string ContainerName { get; } = "salesorderdetail";

        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey("PartitionValue");
    }
}
