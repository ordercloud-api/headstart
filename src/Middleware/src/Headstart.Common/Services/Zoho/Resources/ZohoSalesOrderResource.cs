using Flurl.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Headstart.Common.Services.Zoho.Models;

namespace Headstart.Common.Services.Zoho.Resources
{
	public interface IZohoSalesOrderResource
	{
		Task<ZohoSalesOrderList> ListAsync(params ZohoFilter[] filters);
		Task<TZohoSalesOrderList> ListAsync<TZohoSalesOrderList>(params ZohoFilter[] filters) where TZohoSalesOrderList : ZohoSalesOrderList;
		Task<ZohoSalesOrder> GetAsync(string id);
		Task<TZohoSalesOrder> GetAsync<TZohoSalesOrder>(string id) where TZohoSalesOrder : ZohoSalesOrder;
		Task<ZohoSalesOrder> SaveAsync(ZohoSalesOrder salesOrder);
		Task<TZohoSalesOrder> SaveAsync<TZohoSalesOrder>(TZohoSalesOrder salesOrder) where TZohoSalesOrder : ZohoSalesOrder;
		Task<ZohoSalesOrder> CreateAsync(ZohoSalesOrder salesOrder);
		Task<TZohoSalesOrder> CreateAsync<TZohoSalesOrder>(TZohoSalesOrder salesOrder) where TZohoSalesOrder : ZohoSalesOrder;
		Task DeleteAsync(string id);
	}

	public class ZohoSalesOrderResource : ZohoResource, IZohoSalesOrderResource
	{
		internal ZohoSalesOrderResource(ZohoClient client) : base(client, @"salesorder", @"salesorders")
		{
		}

		public Task<ZohoSalesOrderList> ListAsync(params ZohoFilter[] filters)
		{
			return ListAsync<ZohoSalesOrderList>(filters);
		}

		public Task<TZohoSalesOrderList> ListAsync<TZohoSalesOrderList>(params ZohoFilter[] filters) where TZohoSalesOrderList : ZohoSalesOrderList
		{
			return Get().SetQueryParams(filters?.Select(f => new KeyValuePair<string, object>(f.Key, f.Value))).GetJsonAsync<TZohoSalesOrderList>();
		}

		public Task<ZohoSalesOrder> GetAsync(string id)
		{
			return GetAsync<ZohoSalesOrder>(id);
		}

		public Task<TZohoSalesOrder> GetAsync<TZohoSalesOrder>(string id) where TZohoSalesOrder : ZohoSalesOrder
		{
			return Get(id).GetJsonAsync<TZohoSalesOrder>();
		}

		public Task<ZohoSalesOrder> SaveAsync(ZohoSalesOrder salesOrder)
		{
			return SaveAsync<ZohoSalesOrder>(salesOrder);
		}

		public async Task<TZohoSalesOrder> SaveAsync<TZohoSalesOrder>(TZohoSalesOrder salesOrder) where TZohoSalesOrder : ZohoSalesOrder
		{
			return await Put<TZohoSalesOrder>(salesOrder, salesOrder.salesorder_id);
		}

		public Task<ZohoSalesOrder> CreateAsync(ZohoSalesOrder salesOrder)
		{
			return CreateAsync<ZohoSalesOrder>(salesOrder);
		}

		public async Task<TZohoSalesOrder> CreateAsync<TZohoSalesOrder>(TZohoSalesOrder salesOrder) where TZohoSalesOrder : ZohoSalesOrder
		{
			return await Post<TZohoSalesOrder>(salesOrder);
		}

		public Task DeleteAsync(string id)
		{
			return Delete(id).DeleteAsync();
		}
	}
}