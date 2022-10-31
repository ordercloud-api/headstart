using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public static class HSOrderWorksheetExtensions
    {
        public static bool IsStandardOrder(this HSOrderWorksheet sheet)
        {
            return sheet.Order.xp == null || sheet.Order.xp.OrderType != OrderType.Quote;
        }
    }

    public class HSOrderWorksheet : OrderWorksheet<HSOrder, HSLineItem, HSOrderPromotion, HSShipEstimateResponse, HSOrderCalculateResponse, OrderSubmitResponse, OrderSubmitForApprovalResponse, OrderApprovedResponse>
    {
    }
}
