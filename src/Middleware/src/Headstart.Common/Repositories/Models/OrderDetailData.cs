using ordercloud.integrations.library;
using Headstart.Common.Models.Headstart;

namespace Headstart.Common.Repositories.Models
{
	public class OrderDetailData : CosmosObject
	{
		public string PartitionKey { get; set; } = string.Empty;

		public string OrderId { get; set; } = string.Empty;

		public HsOrder Data { get; set; } = new HsOrder();

		public string ShipFromAddressId { get; set; } = string.Empty;

		public string ShipMethod { get; set; } = string.Empty;

		public string SupplierName { get; set; } = string.Empty;

		public string BrandName { get; set; } = string.Empty;

		public PromotionData Promos { get; set; } = new PromotionData();
	}

	public class PromotionData
	{
		public string PromoCode { get; set; } = string.Empty;

		public string SupplierSpecific { get; set; } = string.Empty;

		public string PromoSupplierName { get; set; } = string.Empty;

		public string OrderLevelPromo { get; set; } = string.Empty;

		public string LineItemLevelPromo { get; set; } = string.Empty;
	}
}