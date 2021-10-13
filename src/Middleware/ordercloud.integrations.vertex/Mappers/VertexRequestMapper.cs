using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;

namespace ordercloud.integrations.vertex
{
	public static class VertexRequestMapper
	{
		public static VertexCalculateTaxRequest ToVertexCalculateTaxRequest(this OrderWorksheet order, List<OrderPromotion> promosOnOrder, string companyCode, VertexSaleMessageType type)
		{
			var itemLines = order.LineItems.Select(ToVertexLineItem);
			var shippingLines = order.ShipEstimateResponse.ShipEstimates.Select(se =>
			{
				var firstLi = order.LineItems.First(li => li.ID == se.ShipEstimateItems.First().LineItemID);
				return ToVertexLineItem(se, firstLi.ShippingAddress);
			});

			return new VertexCalculateTaxRequest() 
			{
				postingDate = DateTime.Now.ToString("yyyy-MM-dd"),
				saleMessageType = type,
				transactionType = VertexTransactionType.SALE,
				transactionId = order.Order.ID,
				seller = new VertexSeller()
				{
					company = companyCode
				},
				customer = new VertexCustomer()
				{
					customerCode = new VertexCustomerCode()
					{
						classCode = order.Order.FromUserID,
						value = order.Order.FromUser.Email
					},
				},
				lineItems = itemLines.Concat(shippingLines).ToList()
			};
		}

		public static VertexLineItem ToVertexLineItem(LineItem lineItem)
		{
			return new VertexLineItem()
			{
				customer = new VertexCustomer()
				{
					destination = lineItem.ShippingAddress.ToVertexLocation(),
				},
				product = new VertexProduct()
				{
					productClass = lineItem.Product.ID,
					value = lineItem.Product.Name
				},
				quantity = new VertexMeasure()
				{
					value = lineItem.Quantity
				},
				unitPrice = lineItem.Quantity,
				lineItemId = lineItem.ID,
				// TODO
				// deliveryTerm = VertexDelveryTerm.FOB 
			};
		}

		public static VertexLineItem ToVertexLineItem(ShipEstimate shipEstimate, Address shipTo)
		{
			var selectedMethod = shipEstimate.ShipMethods.First(m => m.ID == shipEstimate.SelectedShipMethodID);
			return new VertexLineItem()
			{
				customer = new VertexCustomer()
				{
					destination = shipTo.ToVertexLocation(),
				},
				product = new VertexProduct()
				{
					productClass = "shipping_code",
					value = "LineItem for the cost of shipping"
				},
				quantity = new VertexMeasure()
				{
					value = 1
				},
				unitPrice = (double) selectedMethod.Cost,
				lineItemId = shipEstimate.ID,
				// TODO
				// deliveryTerm = VertexDelveryTerm.FOB 
			};
		}

		public static VertexLocation ToVertexLocation(this Address address)
		{
			return new VertexLocation()
			{
				streetAddress1 = address.Street1,
				streetAddress2 = address.Street2,
				city = address.City,
				mainDivision = address.State,
				postalCode = address.Zip,
				country = address.Country
			};
		}

		// OrderCloud stores 2-letter country codes and Vertex needs 3-letter codes.
		public static string ToCountryCode3Letters(this string countryCode2Letters)
		{
			if (countryCode2Letters.Length != 2)
			{
				throw new ArgumentException("country must be two letters.");
			}

			countryCode2Letters = countryCode2Letters.ToUpper();

			CultureInfo[] cultures = CultureInfo.GetCultures(CultureTypes.SpecificCultures);
			foreach (CultureInfo culture in cultures)
			{
				RegionInfo region = new RegionInfo(culture.LCID);
				if (region.TwoLetterISORegionName.ToUpper() == countryCode2Letters)
				{
					return region.ThreeLetterISORegionName;
				}
			}

			return null;
		}

	}
}
