using Headstart.Models;
using ordercloud.integrations.library;

namespace Headstart.Common.Models
{
    public class OrderDetailData : CosmosObject
    {
        public string PartitionKey { get; set; } = string.Empty;

        public string OrderID { get; set; } = string.Empty;

        public HSOrder Data { get; set; } = new HSOrder();

        public string ShipFromAddressID { get; set; } = string.Empty;

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