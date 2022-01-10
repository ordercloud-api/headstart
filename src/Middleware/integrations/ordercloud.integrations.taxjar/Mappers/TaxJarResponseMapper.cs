using ordercloud.integrations.library;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taxjar;
using TaxJarOrder = Taxjar.Order;

namespace ordercloud.integrations.taxjar
{
	public static class TaxJarResponseMapper
	{
		public static OrderTaxCalculation ToOrderTaxCalculation(this IEnumerable<(TaxJarOrder request, TaxResponseAttributes response)> responses)
		{
			var itemLines = responses.Where(r => r.request.LineItems.First().ProductIdentifier != "shipping_code");
			var shippingLines = responses.Where(r => r.request.LineItems.First().ProductIdentifier == "shipping_code");

			return new OrderTaxCalculation()
			{
				OrderID = responses.First().request.TransactionId.Split('|')[1],
				ExternalTransactionID = null, // There are multiple external transactionIDs 
				TotalTax = responses.Select(r => r.response.AmountToCollect).Sum(),
				LineItems = itemLines.Select(ToItemTaxDetails).ToList(),
				OrderLevelTaxes = shippingLines.SelectMany(ToShippingTaxDetails).ToList()
			};
		}

		private static LineItemTaxCalculation ToItemTaxDetails((TaxJarOrder request, TaxResponseAttributes response) taxJarOrder)
		{
			return new LineItemTaxCalculation()
			{
				LineItemID = taxJarOrder.request.TransactionId.Split('|')[3],
				LineItemTotalTax = taxJarOrder.response.AmountToCollect,
				LineItemLevelTaxes = ToTaxDetails(taxJarOrder.response)
			};
		}

		private static List<TaxDetails> ToShippingTaxDetails((TaxJarOrder request, TaxResponseAttributes response) taxJarOrder)
		{
			var taxes = ToTaxDetails(taxJarOrder.response);
			var shipEstimateID = taxJarOrder.request.TransactionId.Split('|')[3];
			foreach (var tax in taxes)
			{
				tax.ShipEstimateID = shipEstimateID;
 			}
			return taxes;
		}

		private static List<TaxDetails> ToTaxDetails(TaxResponseAttributes taxResponse)
		{
			var breakdown = taxResponse.Breakdown.LineItems.First();
			var jurisidctions = new List<TaxDetails>();
			if (breakdown.CountyAmount > 0)
			{
				jurisidctions.Add(new TaxDetails()
				{
					Tax = breakdown.CountyAmount,
					Taxable = breakdown.CountyTaxableAmount,
					Exempt = 0,
					JurisdictionLevel = "County",
					JurisdictionValue = taxResponse.Jurisdictions.County,
					TaxDescription = $"{taxResponse.Jurisdictions.County} County tax",
					ShipEstimateID = null
				});
			}
			if (breakdown.CityAmount > 0)
			{
				jurisidctions.Add(new TaxDetails()
				{
					Tax = breakdown.CityAmount,
					Taxable = breakdown.CityTaxableAmount,
					Exempt = 0,
					JurisdictionLevel = "City",
					JurisdictionValue = taxResponse.Jurisdictions.City,
					TaxDescription = $"{taxResponse.Jurisdictions.City} City tax",
					ShipEstimateID = null
				});
			}
			if (breakdown.StateAmount > 0)
			{
				jurisidctions.Add(new TaxDetails()
				{
					Tax = breakdown.StateAmount,
					Taxable = breakdown.StateTaxableAmount,
					Exempt = 0,
					JurisdictionLevel = "State",
					JurisdictionValue = taxResponse.Jurisdictions.State,
					TaxDescription = $"{taxResponse.Jurisdictions.State} State tax",
					ShipEstimateID = null
				});
			}
			if (breakdown.SpecialDistrictAmount > 0)
			{
				jurisidctions.Add(new TaxDetails()
				{
					Tax = breakdown.SpecialDistrictAmount,
					Taxable = breakdown.SpecialDistrictTaxableAmount,
					Exempt = 0,
					JurisdictionLevel = "Special District",
					JurisdictionValue = "Special District",
					TaxDescription = "Special District tax",
					ShipEstimateID = null
				});
			}
			return jurisidctions;
		}
	}
}
