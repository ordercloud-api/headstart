using OrderCloud.SDK;
using Headstart.Models;
using Headstart.Models.Headstart;
using System.Collections.Generic;
using ordercloud.integrations.exchangerates;

namespace Headstart.Common.Services.ShippingIntegration.Models
{
    public class HSOrderWorksheet : 
        OrderWorksheet<HSOrder, HSLineItem, HSShipEstimateResponse, HSOrderCalculateResponse, OrderSubmitResponse, OrderSubmitForApprovalResponse, OrderApprovedResponse> { }

    public class HSOrderCalculatePayload
    {
        public HSOrderWorksheet OrderWorksheet { get; set; } = new HSOrderWorksheet();

        public CheckoutIntegrationConfiguration ConfigData { get; set; }
    }

    public class ShipEstimateResponseXP { }

    public class ShipEstimateXP
    {
        public List<HSShipMethod> AllShipMethods { get; set; } = new List<HSShipMethod>();

        public string SupplierID { get; set; } = string.Empty;

        public string ShipFromAddressID { get; set; } = string.Empty;
    }

    public class ShipMethodXP
    {
        public string Carrier { get; set; } = string.Empty; // e.g. "Fedex"

        public string CarrierAccountID { get; set; } = string.Empty;

        public decimal ListRate { get; set; }

        public bool Guaranteed { get; set; }

        public decimal OriginalCost { get; set; }

        public bool FreeShippingApplied { get; set; }

        public int? FreeShippingThreshold { get; set; }

        public CurrencySymbol? OriginalCurrency { get; set; }

        public CurrencySymbol? OrderCurrency { get; set; }

        public double? ExchangeRate { get; set; }
    }

    public class HSShipMethod : ShipMethod<ShipMethodXP> { }

    public class HSShipEstimate : ShipEstimate<ShipEstimateXP, HSShipMethod> { }

    public class HSShipEstimateResponse : ShipEstimateResponse<ShipEstimateResponseXP, HSShipEstimate> { }

    public class CheckoutIntegrationConfiguration
    {
        public bool ExcludePOProductsFromShipping { get; set; }

        public bool ExcludePOProductsFromTax { get; set; }
    }

    public static class HSOrderWorksheetExtensions
    {
        public static bool IsStandardOrder(this HSOrderWorksheet sheet)
        {
            return sheet.Order.xp == null || sheet.Order.xp.OrderType != OrderType.Quote;
        }
    }
}