using Headstart.Models;
using ordercloud.integrations.library;

namespace Headstart.Common.Models
{
    public class OrderDetailData : CosmosObject
    {
        public string PartitionKey { get; set; }
        public string OrderID { get; set; }
        public HSOrder Data { get; set; }
        public string ShipFromAddressID { get; set; }
        public string ShipMethod { get; set; }
        public string SupplierName { get; set; }
        public string BrandName { get; set; }
        public PromotionData Promos { get; set; }
    }

    public class PromotionData
    {
        public string PromoCode { get; set; }
        public string SupplierSpecific { get; set; }
        public string PromoSupplierName { get; set; }
        public string OrderLevelPromo { get; set; }
        public string LineItemLevelPromo { get; set; }
    }
}
