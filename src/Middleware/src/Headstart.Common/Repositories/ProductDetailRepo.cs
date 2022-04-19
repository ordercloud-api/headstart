using Microsoft.Azure.Cosmos;
using ordercloud.integrations.library;
using Headstart.Common.Repositories.Models;

namespace Headstart.Common.Repositories
{
	public interface IProductDetailDataRepo : IRepository<ProductDetailData>
	{
	}

	public class ProductDetailDataRepo : CosmosDbRepository<ProductDetailData>, IProductDetailDataRepo
	{
		public override string ContainerName { get; } = @"productdetail";

		public override PartitionKey ResolvePartitionKey(string entityId)
		{
			return new PartitionKey(@"PartitionValue");
		}

		public ProductDetailDataRepo(ICosmosDbContainerFactory factory) : base(factory)
		{
		}
	}
}