using Headstart.Common.Extensions;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Models;
using ordercloud.integrations.exchangerates;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Headstart.Common.Services
{
    public interface IOrderCalcService
    {
        decimal GetCreditCardTotal(HSOrderWorksheet worksheet);
        decimal GetPurchaseOrderTotal(HSOrderWorksheet worksheet);
    }

    public class OrderCalcService : IOrderCalcService
    {
        private readonly IOrderCloudClient _oc;
        public OrderCalcService(IOrderCloudClient oc)
        {
            _oc = oc;
        }

        public decimal GetCreditCardTotal(HSOrderWorksheet worksheet)
        {
            var purchaseOrderTotal = GetPurchaseOrderTotal(worksheet);
            return worksheet.Order.Total - purchaseOrderTotal;
        }

        public decimal GetPurchaseOrderTotal(HSOrderWorksheet worksheet)
        {
            var shippingTotal = GetPurchaseOrderShipping(worksheet);
            return worksheet.GetLineItemsByProductType(ProductType.PurchaseOrder)
                .Where(li => li.Product.xp.ProductType == ProductType.PurchaseOrder)
                .Select(li => li.LineTotal)
                .Sum() + shippingTotal;
        }

        public decimal GetPurchaseOrderShipping(HSOrderWorksheet worksheet)
        {
            decimal POShippingCosts = 0;
            var POlineItemIDs = worksheet.GetLineItemsByProductType(ProductType.PurchaseOrder)?.Select(li => li.ID);
            if (POlineItemIDs?.Count() > 0)
            {
                foreach (var estimate in worksheet?.ShipEstimateResponse?.ShipEstimates)
                {
                    var relatedLineItemIDs = estimate?.ShipEstimateItems?.Select(item => item?.LineItemID);
                    var selectedShipMethod = estimate?.ShipMethods?.FirstOrDefault(method => method?.ID == estimate?.SelectedShipMethodID);
                    if (relatedLineItemIDs != null && relatedLineItemIDs.Any(id => POlineItemIDs.Contains(id)))
                    {
                        POShippingCosts = POShippingCosts + (selectedShipMethod?.Cost ?? 0);
                    }
                }
            }
            return POShippingCosts;
        }
    }
}
    