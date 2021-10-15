using ordercloud.integrations.library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ordercloud.integrations.vertex
{
	public static class VertexResponseMapper
	{
		public static OrderTaxCalculation ToOrderTaxCalculation(this VertexCalculateTaxResponse response)
		{
			var shippingLines = response.lineItems?.Where(line => line.product.productClass == "shipping_code") ?? new List<VertexResponseLineItem>();
			var itemLines = response.lineItems?.Where(line => line.product.productClass != "shipping_code") ?? new List<VertexResponseLineItem>();

			return new OrderTaxCalculation()
			{
				OrderID = response.transactionId,
				ExternalTransactionID = response.transactionId,
				TotalTax = (decimal) response.totalTax,
				LineItems = itemLines.Select(ToItemTaxDetails).ToList(),
				OrderLevelTaxes = shippingLines.SelectMany(ToShippingTaxDetails).ToList()
			};
		}

		public static IEnumerable<TaxDetails> ToShippingTaxDetails(this VertexResponseLineItem transactionLineModel)
		{
			return transactionLineModel.taxes?.Select(detail => detail.ToTaxDetails(transactionLineModel.lineItemId)) ?? new List<TaxDetails>();
		}

		public static LineItemTaxCalculation ToItemTaxDetails(this VertexResponseLineItem transactionLineModel)
		{
			return new LineItemTaxCalculation()
			{
				LineItemID = transactionLineModel.lineItemId,
				LineItemTotalTax = (decimal) transactionLineModel.totalTax,
				LineItemLevelTaxes = transactionLineModel.taxes?.Select(detail => detail.ToTaxDetails(null)).ToList() ?? new List<TaxDetails>()
			};
		}

		public static TaxDetails ToTaxDetails(this VertexTax detail, string shipEstimateID)
		{
			return new TaxDetails()
			{
				Tax = (decimal) detail.calculatedTax,
				Taxable = (decimal) detail.taxable,
				Exempt = 0, // we don't get a property back for exempt
				TaxDescription = detail.impositionType.value,
				JurisdictionLevel = detail.jurisdiction.jurisdictionLevel.ToString(),
				JurisdictionValue = detail.jurisdiction.value,
				ShipEstimateID = shipEstimateID
			};
		}
	}
}
