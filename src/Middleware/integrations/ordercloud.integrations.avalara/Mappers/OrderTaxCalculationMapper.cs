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
			var shippingLines = avalaraTransaction.lines.GroupBy(line => line.taxCode == "FR");
			return new OrderTaxCalculation()
			{
				OrderCloudOrderID = avalaraTransaction.purchaseOrderNo,
				ExternalSystemTransactionID = avalaraTransaction.code,
				TotalTax = avalaraTransaction.totalTax ?? 0,
				TotalTaxable = avalaraTransaction.totalTaxable ?? 0,
				LineItems = null,
				OrderLevelTaxes = null
			};
		}

	}
}
