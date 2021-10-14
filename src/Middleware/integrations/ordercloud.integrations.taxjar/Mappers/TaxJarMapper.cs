using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taxjar;
using TaxJarLineItem = Taxjar.LineItem;
using TaxJarOrder = Taxjar.Order;

namespace ordercloud.integrations.taxjar.Mappers
{
	public static class TaxJarMapper
	{
		/// <summary>
		///	Returns a list of TaxJarOrders because each order has a single to and from address. They therefor coorespond to OrderCloud LineItems. 
		/// </summary>
		public static List<TaxJarOrder> ToTaxJarOrders(OrderWorksheet order)
		{
			var itemLines = order.LineItems.Select(li =>
			{
				return new TaxJarOrder()
				{
					TransactionId = $"{order.Order.ID}|{li.ID}",
					Amount = li.LineTotal,
					Shipping = 0, // will create separate lines for shipping

					FromCity = li.ShipFromAddress.City,
					FromZip = li.ShipFromAddress.Zip,
					FromState = li.ShipFromAddress.State,
					FromCountry = li.ShipFromAddress.Country,
					FromStreet = li.ShipFromAddress.Street1,

					ToCity = li.ShippingAddress.City,
					ToZip = li.ShippingAddress.Zip,
					ToState = li.ShippingAddress.State,
					ToCountry = li.ShippingAddress.Country,
					ToStreet = li.ShippingAddress.Street1,

					LineItems = new List<LineItem> { }

				};
			});
		}
	}
}
