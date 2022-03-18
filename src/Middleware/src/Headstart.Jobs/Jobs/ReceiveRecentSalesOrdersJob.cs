using Headstart.Common.Models;
using Headstart.Common.Models.Headstart;
using Headstart.Common.Repositories;
using Headstart.Common.Repositories.Models;
using Headstart.Jobs.Helpers;
using OrderCloud.SDK;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Headstart.Jobs
{
	public class ReceiveRecentSalesOrdersJob : BaseReportJob
	{
		private readonly IOrderCloudClient _oc;
		private readonly ISalesOrderDetailDataRepo _salesOrderDetailDataRepo;

		public ReceiveRecentSalesOrdersJob(IOrderCloudClient oc, ISalesOrderDetailDataRepo salesOrderDetailDataRepo) 
		{
			_oc = oc;
			_salesOrderDetailDataRepo = salesOrderDetailDataRepo;
		}

		protected override async Task<ResultCode> ProcessJobAsync(string message)
		{
			try
			{
				await UpsertSalesOrderDetail(message);
				return ResultCode.Success;
			}
			catch (Exception ex)
			{
				LogFailure($@"{ex.Message}. {ex?.InnerException?.Message}. {ex.StackTrace}.");
				return ResultCode.PermanentFailure;
			}
		}

		private async Task UpsertSalesOrderDetail(string orderId)
		{
			var order = await _oc.Orders.GetAsync<HsOrder>(OrderDirection.Incoming, orderId);
			var brand = await _oc.Buyers.GetAsync<HsBuyer>(order.FromCompanyID);
			var promos = await _oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderId);
			var cosmosSalesOrder = new OrderDetailData()
			{
				PartitionKey = @"PartitionValue",
				OrderId = orderId,
				Data = order,
				BrandName = brand.Name
			};

			if (promos.Items.Count > 0)
			{
				cosmosSalesOrder.Promos = ReportPromoBuilder.BuildPromoFields(promos, ReportTypeEnum.SalesOrderDetail);
			}
			var queryable = _salesOrderDetailDataRepo.GetQueryable().Where(order => order.PartitionKey == @"PartitionValue");
			var requestOptions = BuildQueryRequestOptions();
			var listOptions = BuildListOptions(orderId);
			var currentOrderListPage = await _salesOrderDetailDataRepo.GetItemsAsync(queryable, requestOptions, listOptions);
			var cosmosId = string.Empty;
			if (currentOrderListPage.Items.Count() == 1)
			{
				cosmosId = cosmosSalesOrder.id = currentOrderListPage.Items[0].id;
			}
			await _salesOrderDetailDataRepo.UpsertItemAsync(cosmosId, cosmosSalesOrder);
		}
	}
}
