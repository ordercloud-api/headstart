using Avalara.AvaTax.RestClient;
using ordercloud.integrations.library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ordercloud.integrations.avalara
{
	public static class OrderTaxCalculationMapper
	{
		public static OrderTaxCalculation ToOrderTaxCalculation(this TransactionModel avalaraTransaction)
		{
			var shippingLines = avalaraTransaction.lines?.Where(line => line.taxCode == "FR") ?? new List<TransactionLineModel>();
			var itemLines = avalaraTransaction.lines?.Where(line => line.taxCode != "FR") ?? new List<TransactionLineModel>();
			return new OrderTaxCalculation()
			{
				OrderID = avalaraTransaction.purchaseOrderNo,
				ExternalTransactionID = avalaraTransaction.code,
				TotalTax = avalaraTransaction.totalTax ?? 0,
				LineItems = itemLines.Select(ToItemTaxDetails).ToList(),
				OrderLevelTaxes = shippingLines.SelectMany(ToShippingTaxDetails).ToList()
			};
		}

		public static IEnumerable<TaxDetails> ToShippingTaxDetails(this TransactionLineModel transactionLineModel)
		{
			return transactionLineModel.details?.Select(detail => detail.ToTaxDetails(transactionLineModel.lineNumber)) ?? new List<TaxDetails>();
		}

		public static LineItemTaxCalculation ToItemTaxDetails(this TransactionLineModel transactionLineModel)
		{
			return new LineItemTaxCalculation()
			{
				LineItemID = transactionLineModel.lineNumber,
				LineItemTotalTax = transactionLineModel.taxCalculated ?? 0,
				LineItemLevelTaxes = transactionLineModel.details?.Select(detail => detail.ToTaxDetails(null)).ToList() ?? new List<TaxDetails>()
			};
		}

		public static TaxDetails ToTaxDetails(this TransactionLineDetailModel detail, string shipEstimateID)
		{
			return new TaxDetails()
			{
				Tax = detail.tax ?? 0,
				Taxable = detail.taxableAmount ?? 0,
				Exempt = detail.exemptAmount ?? 0,
				TaxDescription = detail.taxName,
				JurisdictionLevel = detail.jurisdictionType.ToString(),
				JurisdictionValue = detail.jurisName,
				ShipEstimateID = shipEstimateID
			};
		}
	}
}
