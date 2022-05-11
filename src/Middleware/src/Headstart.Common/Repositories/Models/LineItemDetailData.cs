using Headstart.Models;
using ordercloud.integrations.library;

namespace Headstart.Common.Models
{
    public class LineItemDetailData : CosmosObject
    {
        public string PartitionKey { get; set; }
        public string OrderID { get; set; }
        public HSOrderLineItemData Data { get; set; }
    }
}
