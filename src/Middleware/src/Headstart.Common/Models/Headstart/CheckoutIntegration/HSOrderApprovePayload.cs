using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class HSOrderApprovePayload : WebhookPayloads.Orders.Approve<dynamic, OrderApprovalInfo, HSOrder>
    {
    }
}
