using Headstart.Models;
using ordercloud.integrations.library;

namespace Headstart.Common.Models
{
    public class LineItemDetailData : CosmosObject
    {
        public string PartitionKey { get; set; } = string.Empty;

        public string OrderID { get; set; } = string.Empty;

        public HSOrderLineItemData Data { get; set; } = new HSOrderLineItemData();
    }
}