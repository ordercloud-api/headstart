using Microsoft.Azure.Cosmos;
using OrderCloud.Integrations.CosmosDB;
using OrderCloud.Integrations.Reporting.Models;

namespace OrderCloud.Integrations.Reporting.Repositories
{
    public interface ILineItemDetailDataRepo : IRepository<LineItemDetailData>
    {
    }

    public class LineItemDetailDataRepo : CosmosDbRepository<LineItemDetailData>, ILineItemDetailDataRepo
    {
        public LineItemDetailDataRepo(ICosmosDbContainerFactory factory)
            : base(factory)
        {
        }

        public override string ContainerName { get; } = "lineitemdetail";

        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey("PartitionValue");
    }
}
