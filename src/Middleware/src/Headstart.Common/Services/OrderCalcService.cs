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
            return worksheet.LineItems
                .Where(li => li.Product.xp.ProductType == ProductType.PurchaseOrder)
                .Select(li => li.LineTotal)
                .Sum();
        }
    }
}
    