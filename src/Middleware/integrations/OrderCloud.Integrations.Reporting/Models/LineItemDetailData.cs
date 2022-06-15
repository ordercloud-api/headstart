using Headstart.Common.Models;
using OrderCloud.Integrations.CosmosDB;

namespace OrderCloud.Integrations.Reporting.Models
{
    public class LineItemDetailData : CosmosObject
    {
        public string PartitionKey { get; set; }

        public string OrderID { get; set; }

        public HSOrderLineItemData Data { get; set; }
    }
}
