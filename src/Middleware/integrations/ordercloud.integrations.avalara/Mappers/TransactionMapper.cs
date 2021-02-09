using Avalara.AvaTax.RestClient;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ordercloud.integrations.avalara
{
	public static class TransactionMapper
	{
		public static CreateTransactionModel ToAvalaraTransationModel(this OrderWorksheet order, string companyCode, DocumentType docType)
		{
			var buyerLocationID = order.Order.BillingAddress.ID;

			var standardLineItems = order.LineItems.Where(li => li.Product.xp.ProductType == "Standard")?.ToList();
			var poLineItemIDs = order.LineItems.Where(li => li.Product.xp.ProductType == "PurchaseOrder")?.Select(li => li?.ID)?.ToList();
			var standardShipEstimates = order.ShipEstimateResponse.ShipEstimates.Where(estimate =>
			{
				return !estimate.ShipEstimateItems.Any(item => poLineItemIDs.Contains(item.LineItemID));
			});

			var shipingLines = standardShipEstimates.Select(shipment =>
			{
				var (shipFrom, shipTo) = shipment.GetAddresses(order.LineItems);
				var method = shipment.GetSelectedShippingMethod();
				return method.ToLineItemModel(shipFrom, shipTo);
			});

			var hasResaleCert = ((int?) order.Order.BillingAddress.xp.AvalaraCertificateID != null);
			var exemptionNo = hasResaleCert ? buyerLocationID : null;

			var productLines = standardLineItems.Select(lineItem =>
				 lineItem.ToLineItemModel(lineItem.ShipFromAddress, lineItem.ShippingAddress, exemptionNo));

			return new CreateTransactionModel()
			{
				companyCode = companyCode,
				type = docType,
				customerCode = buyerLocationID,
				date = DateTime.Now,
				lines = productLines.Concat(shipingLines).ToList(),
				purchaseOrderNo = order.Order.ID
			};
		}


		private static LineItemModel ToLineItemModel(this LineItem lineItem, Address shipFrom, Address shipTo, string exemptionNo)
		{
			var line = new LineItemModel()
			{
				amount = lineItem.LineTotal,
				quantity = lineItem.Quantity,
				taxCode = lineItem.Product.xp.Tax.Code,
				itemCode = lineItem.ProductID,
				customerUsageType = null,
				number = lineItem.ID,
				addresses = ToAddressesModel(shipFrom, shipTo)
			};
			var isResaleProduct = (bool)lineItem.Product.xp.IsResale;
			if (isResaleProduct && exemptionNo != null)
			{
				line.exemptionCode = exemptionNo;
			}
			return line;
		}

		private static LineItemModel ToLineItemModel(this ShipMethod method, Address shipFrom, Address shipTo)
		{
			return new LineItemModel()
			{
				amount = method.Cost,
				taxCode = "FR",
				itemCode = method.Name,
				customerUsageType = null,
				number = method.ID,
				addresses = ToAddressesModel(shipFrom, shipTo)
			};
		}

		private static AddressesModel ToAddressesModel(Address shipFrom, Address shipTo)
		{
			return new AddressesModel()
			{
				shipFrom = shipFrom.ToAddressLocationInfo(),
				shipTo = shipTo.ToAddressLocationInfo(),
			};
		}

		private static AddressLocationInfo ToAddressLocationInfo(this Address address)
		{
			return new AddressLocationInfo()
			{
				line1 = address.Street1,
				line2 = address.Street2,
				city = address.City,
				region = address.State,
				postalCode = address.Zip,
				country = address.Country
			};
		}
	}
}
