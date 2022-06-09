using Microsoft.Azure.Cosmos;
using OrderCloud.Integrations.CosmosDB;
using OrderCloud.Integrations.Reporting.Models;

namespace OrderCloud.Integrations.Reporting.Repositories
{
    public interface IProductDetailDataRepo : IRepository<ProductDetailData>
    {
    }

    public class ProductDetailDataRepo : CosmosDbRepository<ProductDetailData>, IProductDetailDataRepo
    {
        public ProductDetailDataRepo(ICosmosDbContainerFactory factory)
            : base(factory)
        {
        }

        public override string ContainerName { get; } = "productdetail";

        public override PartitionKey ResolvePartitionKey(string entityId) => new PartitionKey("PartitionValue");
    }
}
