using Microsoft.Azure.Cosmos;
using Headstart.Common.Models;
using ordercloud.integrations.library;

namespace Headstart.Common.Repositories
{
    public interface ISalesOrderDetailDataRepo : IRepository<OrderDetailData> { }

    public class SalesOrderDetailDataRepo : CosmosDbRepository<OrderDetailData>, ISalesOrderDetailDataRepo
    {
        public override string ContainerName { get; } = $@"salesorderdetail";
        public override PartitionKey ResolvePartitionKey(string entityId)
        {
            return new PartitionKey($@"PartitionValue");
        }

        public SalesOrderDetailDataRepo(ICosmosDbContainerFactory factory) : base(factory) { }
    }
}