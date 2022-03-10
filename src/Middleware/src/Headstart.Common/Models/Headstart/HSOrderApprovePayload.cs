using OrderCloud.SDK;

namespace Headstart.Common.Models.Headstart
{
	public class HSOrderApprovePayload : WebhookPayloads.Orders.Approve<dynamic, OrderApprovalInfo, HsOrder>
	{
	}
}