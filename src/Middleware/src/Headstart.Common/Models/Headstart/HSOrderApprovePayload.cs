using OrderCloud.SDK;

namespace Headstart.Models
{
    public class HSOrderApprovePayload : WebhookPayloads.Orders.Approve<dynamic, OrderApprovalInfo, HSOrder> { }
}
