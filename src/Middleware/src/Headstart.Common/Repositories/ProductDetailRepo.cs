using Headstart.Common.Repositories.Models;
using Microsoft.Azure.Cosmos;
using OrderCloud.Integrations.Library;

namespace Headstart.Common.Repositories
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
