using Headstart.Common.Models.Headstart;
using ordercloud.integrations.library;

namespace Headstart.Common.Repositories.Models
{
	public class LineItemDetailData : CosmosObject
	{
		public string PartitionKey { get; set; } = string.Empty;

		public string OrderId { get; set; } = string.Empty;

		public HsOrderLineItemData Data { get; set; } = new HsOrderLineItemData();
	}
}