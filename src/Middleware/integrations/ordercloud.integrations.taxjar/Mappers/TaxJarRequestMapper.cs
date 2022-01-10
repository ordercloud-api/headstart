using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taxjar;
using OCLineItem = OrderCloud.SDK.LineItem;
using TaxJarLineItem = Taxjar.LineItem;
using TaxJarOrder = Taxjar.Order;

namespace ordercloud.integrations.taxjar
{
	public static class TaxJarRequestMapper
	{
		/// <summary>
		///	Returns a list of TaxJarOrders because each order has a single to and from address. They therefor coorespond to OrderCloud LineItems. 
		/// </summary>
		public static List<TaxJarOrder> ToTaxJarOrders(this OrderWorksheet order)
		{
			var itemLines = order.LineItems.Select(li => ToTaxJarOrders(li, order.Order.ID));
			var shippingLines = order.ShipEstimateResponse.ShipEstimates.Select(se =>
			{
				var firstLineItem = order.LineItems.First(li => li.ID == se.ShipEstimateItems.First().LineItemID);
				return ToTaxJarOrders(se, firstLineItem, order.Order.ID);
			});
			return itemLines.Concat(shippingLines).ToList();
		}

		private static TaxJarOrder ToTaxJarOrders(ShipEstimate shipEstimate, OCLineItem lineItem, string orderID)
		{
			var selectedShipMethod = shipEstimate.ShipMethods.First(x => x.ID == shipEstimate.SelectedShipMethodID);
			return new TaxJarOrder()
			{
				TransactionId = $"OrderID:|{orderID}|ShippingEstimateID:|{shipEstimate.ID}",
				Shipping = 0, // will create separate lines for shipping

				FromCity = lineItem.ShipFromAddress.City,
				FromZip = lineItem.ShipFromAddress.Zip,
				FromState = lineItem.ShipFromAddress.State,
				FromCountry = lineItem.ShipFromAddress.Country,
				FromStreet = lineItem.ShipFromAddress.Street1,

				ToCity = lineItem.ShippingAddress.City,
				ToZip = lineItem.ShippingAddress.Zip,
				ToState = lineItem.ShippingAddress.State,
				ToCountry = lineItem.ShippingAddress.Country,
				ToStreet = lineItem.ShippingAddress.Street1,

				LineItems = new List<TaxJarLineItem> {
						new TaxJarLineItem()
						{
							Id = shipEstimate.ID,
							Quantity = 1,
							UnitPrice = selectedShipMethod.Cost,
							Description = selectedShipMethod.Name,
							ProductIdentifier = "shipping_code",
						}
					}
			};
		}

		private static TaxJarOrder ToTaxJarOrders(OCLineItem lineItem, string orderID)
		{
			return new TaxJarOrder()
			{
				TransactionId = $"OrderID:|{orderID}|LineItemID:|{lineItem.ID}",
				Shipping = 0, // will create separate lines for shipping

				FromCity = lineItem.ShipFromAddress.City,
				FromZip = lineItem.ShipFromAddress.Zip,
				FromState = lineItem.ShipFromAddress.State,
				FromCountry = lineItem.ShipFromAddress.Country,
				FromStreet = lineItem.ShipFromAddress.Street1,

				ToCity = lineItem.ShippingAddress.City,
				ToZip = lineItem.ShippingAddress.Zip,
				ToState = lineItem.ShippingAddress.State,
				ToCountry = lineItem.ShippingAddress.Country,
				ToStreet = lineItem.ShippingAddress.Street1,

				LineItems = new List<TaxJarLineItem> {
						new TaxJarLineItem()
						{
							Id = lineItem.ID,
							Quantity = lineItem.Quantity,
							UnitPrice = lineItem.UnitPrice ?? 0,
							Description = lineItem.Product.Name,
							ProductIdentifier = lineItem.Product.ID,
						}
					}
			};
		}
	}
}
