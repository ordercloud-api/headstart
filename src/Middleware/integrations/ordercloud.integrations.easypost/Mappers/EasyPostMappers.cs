using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ordercloud.integrations.library;

namespace ordercloud.integrations.easypost
{
	public static class EasyPostMappers
	{
		public static EasyPostAddress MapAddress(Address address)
		{
			return new EasyPostAddress()
			{
				street1 = address.Street1,
				street2 = address.Street2,
				city = address.City,
				state = address.State,
				zip = address.Zip,
				country = address.Country,
			};
		}

		// To use this method all the LineItems should have the same ShipTo and ShipFrom
		// TODO - does this need to be more intelligient
		//public static EasyPostParcel MapParcel(IList<LineItem> lineItems)
		//{
		//	var aggregateHeight = (double)Math.Min(MINIMUM_SHIP_DIMENSION, lineItems.Max(li => li.Product.ShipHeight ?? MINIMUM_SHIP_DIMENSION));
		//	var aggregateWidth = (double)Math.Min(MINIMUM_SHIP_DIMENSION, lineItems.Max(li => li.Product.ShipWidth ?? MINIMUM_SHIP_DIMENSION));
		//	var aggregateLength = (double)Math.Min(MINIMUM_SHIP_DIMENSION, lineItems.Max(li => li.Product.ShipLength ?? MINIMUM_SHIP_DIMENSION));
		//	var totalWeight = lineItems.Aggregate(0.0, (sum, lineItem) => 
		//	{
		//		var productShipWeight = lineItem.Product.ShipWeight ?? 1;
		//		return sum += ((double)productShipWeight * lineItem.Quantity);
		//	});
		//	return new EasyPostParcel() { 
		//		weight = totalWeight,
		//		height = aggregateHeight,
		//		width = aggregateWidth,
		//		length = aggregateLength
		//	};
		//}

        private static List<EasyPostCustomsItem> MapCustomsItem(IGrouping<AddressPair, LineItem> lineitems, EasyPostShippingProfile profile)
        {
            return lineitems.Select(lineItem => new EasyPostCustomsItem()
                {
                    description = lineItem.Product.Name,
                    hs_tariff_number = profile.HS_Tariff_Number,
                    origin_country = lineItem.ShipFromAddress.Country,
                    value = decimal.ToDouble(lineItem.LineSubtotal),
                    quantity = lineItem.Quantity,
                    weight = lineItem.ShipWeightOrDefault(Package.DEFAULT_WEIGHT)
                })
                .ToList();
        }


		public static IList<ShipMethod> MapRates(EasyPostShipment[] shipments)
		{
			return shipments
				.SelectMany(shipment => shipment.rates)
				.GroupBy(easyPostRate => new { easyPostRate.service, easyPostRate.carrier, easyPostRate.carrier_account_id })
				.Select(group =>
				{
					var first = group.First();
					var cost = group.Aggregate(0M, (sum, rate) => sum += decimal.Parse(rate.rate));
					var listRate = group.Aggregate(0M, (sum, rate) => sum += decimal.Parse(rate.list_rate));
					var deliveryDays = group.Max(rate => rate.delivery_days ?? rate.est_delivery_days ?? 10);
					var guaranteedDeliveryDays = group.Max(rate => rate.delivery_date_guaranteed);

					return new ShipMethod()
					{
						ID = first.id,
						Name = group.Key.service,
						Cost = cost,
						EstimatedTransitDays = deliveryDays,
						xp =
						{
							Carrier = group.Key.carrier,
							CarrierAccountID = group.Key.carrier_account_id,
							ListRate = listRate,
							Guaranteed = guaranteedDeliveryDays,
							OriginalCost = cost
						}
					};
				}).ToList();
		}

		public static List<EasyPostShipment> MapShipment(IGrouping<AddressPair, LineItem> groupedLineItems, EasyPostShippingProfiles profiles)
		{
			var parcels = SmartPackageMapper.MapLineItemsIntoPackages(groupedLineItems.ToList());
			var shipments = parcels.Select(parcel =>
			{
				var shipment = new EasyPostShipment()
				{
					from_address = MapAddress(groupedLineItems.Key.ShipFrom),
					to_address = MapAddress(groupedLineItems.Key.ShipTo),
					parcel = parcel, // All line items with the same shipFrom and shipTo are grouped into 1 "parcel"
					carrier_accounts = new List<EasyPostCarrierAccount>()
                    //carrier_accounts = profiles.ShippingProfiles.Select(id =>  new EasyPostCarrierAccount() { id = id.CarrierAccountIDs })
				};
				foreach (var p in profiles.ShippingProfiles)
                {
                    p.CarrierAccountIDs.ForEach(i => shipment.carrier_accounts.Add(new EasyPostCarrierAccount() { id = i }));
                }

				// add customs info for international shipments
				if (groupedLineItems.Key.ShipTo.Country != "US")
				{
					var line_item = groupedLineItems.FirstOrDefault(g => g.SupplierID != null);

					var profile = profiles.FirstOrDefault(line_item?.SupplierID);
					shipment.customs_info = new EasyPostCustomsInfo()
					{
						contents_type = "merchandise",
						restriction_type = profile.Restriction_Type,
						eel_pfc = profile.EEL_PFC,
						customs_certify = profile.Customs_Certify,
						customs_signer = profile.Customs_Signer,
						customs_items = MapCustomsItem(groupedLineItems, profile)
					};
				}
				return shipment;
			}).ToList();

			return shipments;
        }
    }
}
