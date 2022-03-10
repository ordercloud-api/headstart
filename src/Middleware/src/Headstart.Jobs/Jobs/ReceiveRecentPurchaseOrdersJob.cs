using System;
using System.Linq;
using OrderCloud.SDK;
using OrderCloud.Catalyst;
using Headstart.Jobs.Helpers;
using System.Threading.Tasks;
using Headstart.Common.Models;
using System.Collections.Generic;
using Headstart.Common.Repositories;
using ordercloud.integrations.library;
using Headstart.Common.Models.Headstart;
using Headstart.Common.Repositories.Models;

namespace Headstart.Jobs
{
	public class ReceiveRecentPurchaseOrdersJob : BaseReportJob
	{
		private readonly IOrderCloudClient _oc;
		private readonly IPurchaseOrderDetailDataRepo _purchaseOrderDetailDataRepo;

		public ReceiveRecentPurchaseOrdersJob(IOrderCloudClient oc, IPurchaseOrderDetailDataRepo purchaseOrderDetailDataRepo)
		{
			_oc = oc;
			_purchaseOrderDetailDataRepo = purchaseOrderDetailDataRepo;
		}

		protected override async Task<ResultCode> ProcessJobAsync(string message)
		{
			try
			{
				await UpsertPurchaseOrderDetail(message);
				return ResultCode.Success;
			}
			catch (Exception ex)
			{
				LogFailure($"{ex.Message} {ex?.InnerException?.Message} {ex.StackTrace}");
				return ResultCode.PermanentFailure;
			}
		}

		private async Task UpsertPurchaseOrderDetail(string orderID)
		{
			var orders = await _oc.Orders.ListAllAsync<HsOrder>(OrderDirection.Outgoing, filters: $"ID={orderID}-*");

			var queryable = _purchaseOrderDetailDataRepo.GetQueryable().Where(order => order.PartitionKey == "PartitionValue");

			var requestOptions = BuildQueryRequestOptions();

			var salesOrderWorksheet = await _oc.IntegrationEvents.GetWorksheetAsync<HsOrderWorksheet>(OrderDirection.Incoming, orderID);

			var promos = await _oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, salesOrderWorksheet.Order.ID);

			var discountedLineItems = new List<HsLineItem>();

			if (promos.Items.Count() > 0)
			{
				var discountedLineFilter = new Dictionary<string, object>
				{
					["PromotionDiscount"] = ">0"
				};

				discountedLineItems = await _oc.LineItems.ListAllAsync<HsLineItem>(OrderDirection.Incoming, salesOrderWorksheet.Order.ID, filters: discountedLineFilter);
			}

			foreach (var order in orders)
			{
				order.FromUser = salesOrderWorksheet.Order.FromUser;

				order.BillingAddress = new HsAddressBuyer()
				{
					xp = new BuyerAddressXP()
					{
						LocationId = salesOrderWorksheet?.Order?.BillingAddress?.xp?.LocationId
					}
				};

				var brand = await _oc.Buyers.GetAsync<HsBuyer>(salesOrderWorksheet.Order.FromCompanyID);
				var supplier = await _oc.Suppliers.GetAsync<HsSupplier>(order.ToCompanyID);

				order.ShippingCost = GetPurchaseOrderShippingCost(salesOrderWorksheet, order.ToCompanyID);
				if (salesOrderWorksheet.Order.PromotionDiscount > 0)
				{
					order.PromotionDiscount = GetPurchaseOrderPromotionDiscount(salesOrderWorksheet, promos.Items, order.ToCompanyID);
				}

				var cosmosPurchaseOrder = new OrderDetailData()
				{
					PartitionKey = "PartitionValue",
					OrderId = order.ID,
					Data = order,
					SupplierName = supplier.Name,
					BrandName = brand.Name,
				};

				if (promos.Items.Count > 0)
				{
					cosmosPurchaseOrder.Promos = ReportPromoBuilder.BuildPromoFields(promos, ReportTypeEnum.PurchaseOrderDetail, order.ToCompanyID, discountedLineItems);
				}

				var listOptions = BuildListOptions(order.ID);

				CosmosListPage<OrderDetailData> currentOrderListPage = await _purchaseOrderDetailDataRepo.GetItemsAsync(queryable, requestOptions, listOptions);

				var cosmosID = "";
				if (currentOrderListPage.Items.Count() == 1)
				{
					cosmosID = cosmosPurchaseOrder.id = currentOrderListPage.Items[0].id;
				}

				await _purchaseOrderDetailDataRepo.UpsertItemAsync(cosmosID, cosmosPurchaseOrder);
			}
		}

		private decimal GetPurchaseOrderShippingCost(HsOrderWorksheet orderWorksheet, string supplierID)
		{
			var supplierShipEstimate = orderWorksheet?.ShipEstimateResponse?.ShipEstimates?.FirstOrDefault(estimate => estimate?.xp?.SupplierId == supplierID);
			if (supplierShipEstimate == null)
			{
				return 0M;
			}

			if (orderWorksheet?.OrderCalculateResponse?.xp?.TaxCalculation?.OrderLevelTaxes?.FirstOrDefault(line => line?.ShipEstimateID == supplierShipEstimate?.ID) != null)
			{
				var shippingCost = orderWorksheet?.OrderCalculateResponse?.xp?.TaxCalculation?.OrderLevelTaxes?.FirstOrDefault(line => line?.ShipEstimateID == supplierShipEstimate?.ID)?.Tax;
				return shippingCost != null ? Math.Round((decimal)shippingCost, 2) : 0M;
			}

			return 0M;
		}

		private decimal GetPurchaseOrderPromotionDiscount(HsOrderWorksheet orderWorksheet, IEnumerable<OrderPromotion> promosOnOrder, string supplierID)
		{

			var supplierLineItems = orderWorksheet?.LineItems?.Where(line => line?.SupplierID == supplierID);

			if (supplierLineItems == null || supplierLineItems.Count() == 0) { return 0M; }

			var lineItemDiscount = supplierLineItems.Sum(line => line.PromotionDiscount);

			var totalOrderLevelDiscount = promosOnOrder
				.Where(promo => promo.LineItemID == null && !promo.LineItemLevel)
				.Select(promo => promo.Amount).Sum();

			var fractionOfOrderSubtotal = supplierLineItems.Select(l => l.LineSubtotal).Sum() / orderWorksheet.Order.Subtotal;
			var proportionalOrderDiscount = totalOrderLevelDiscount * fractionOfOrderSubtotal;

			return lineItemDiscount + proportionalOrderDiscount;
		}
	}
}
