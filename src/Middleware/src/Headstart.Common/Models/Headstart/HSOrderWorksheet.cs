using OrderCloud.SDK;
using System.Collections.Generic;
using ordercloud.integrations.exchangerates;

namespace Headstart.Common.Models.Headstart
{
	public class HsOrderWorksheet : OrderWorksheet<HsOrder, HsLineItem, HsShipEstimateResponse, HsOrderCalculateResponse, OrderSubmitResponse, OrderSubmitForApprovalResponse, OrderApprovedResponse>
	{
	}

	public class HsOrderCalculatePayload
	{
		public HsOrderWorksheet OrderWorksheet { get; set; } = new HsOrderWorksheet();

		public CheckoutIntegrationConfiguration ConfigData { get; set; } = new CheckoutIntegrationConfiguration();
	}

	public class ShipEstimateResponseXp
	{
	}

	public class ShipEstimateXp
	{
		public List<HsShipMethod> AllShipMethods { get; set; } = new List<HsShipMethod>();

		public string SupplierID { get; set; } = string.Empty;

		public string ShipFromAddressID { get; set; } = string.Empty;
	}

	public class ShipMethodXp
	{
		public string Carrier { get; set; } = string.Empty; // e.g. "Fedex"

		public string CarrierAccountId { get; set; } = string.Empty;

		public decimal ListRate { get; set; }

		public bool Guaranteed { get; set; }

		public decimal OriginalCost { get; set; }

		public bool FreeShippingApplied { get; set; }

		public int? FreeShippingThreshold { get; set; }

		public CurrencySymbol? OriginalCurrency { get; set; }

		public CurrencySymbol? OrderCurrency { get; set; }

		public double? ExchangeRate { get; set; }
	}

	public class HsShipMethod : ShipMethod<ShipMethodXp> { }

	public class HsShipEstimate : ShipEstimate<ShipEstimateXp, HsShipMethod>
	{
	}

	public class HsShipEstimateResponse : ShipEstimateResponse<ShipEstimateResponseXp, HsShipEstimate>
	{
	}

	public class CheckoutIntegrationConfiguration
	{
		public bool ExcludePOProductsFromShipping { get; set; }

		public bool ExcludePOProductsFromTax { get; set; }
	}

	public static class HsOrderWorksheetExtensions
	{
		public static bool IsStandardOrder(this HsOrderWorksheet sheet)
		{
			return sheet.Order.xp == null || sheet.Order.xp.OrderType != OrderType.Quote;
		}
	}
}