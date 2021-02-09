using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Headstart.Common.Services.Zoho.Models;

namespace Headstart.Common.Services.Zoho.Resources
{
    public interface IZohoPurchaseOrderResource
    {
        Task<ZohoPurchaseOrderList> ListAsync(params ZohoFilter[] filters);
        Task<TZohoPurchaseOrderList> ListAsync<TZohoPurchaseOrderList>(params ZohoFilter[] filters) where TZohoPurchaseOrderList : ZohoPurchaseOrderList;
        Task<ZohoPurchaseOrder> GetAsync(string id);
        Task<TZohoPurchaseOrder> GetAsync<TZohoPurchaseOrder>(string id) where TZohoPurchaseOrder : ZohoPurchaseOrder;
        Task<ZohoPurchaseOrder> SaveAsync(ZohoPurchaseOrder purchaseOrder);
        Task<TZohoPurchaseOrder> SaveAsync<TZohoPurchaseOrder>(TZohoPurchaseOrder purchaseOrder) where TZohoPurchaseOrder : ZohoPurchaseOrder;
        Task<ZohoPurchaseOrder> CreateAsync(ZohoPurchaseOrder purchaseOrder);
        Task<TZohoPurchaseOrder> CreateAsync<TZohoPurchaseOrder>(TZohoPurchaseOrder purchaseOrder) where TZohoPurchaseOrder : ZohoPurchaseOrder;
        Task DeleteAsync(string id);
    }

    public class ZohoPurchaseOrderResource : ZohoResource, IZohoPurchaseOrderResource
    {
        internal ZohoPurchaseOrderResource(ZohoClient client) : base(client, "purchaseorder", "purchaseorders") { }

        public Task<ZohoPurchaseOrderList> ListAsync(params ZohoFilter[] filters) => ListAsync<ZohoPurchaseOrderList>(filters);
        public Task<TZohoPurchaseOrderList> ListAsync<TZohoPurchaseOrderList>(params ZohoFilter[] filters) where TZohoPurchaseOrderList : ZohoPurchaseOrderList => Get()
                .SetQueryParams(filters?.Select(f => new KeyValuePair<string, object>(f.Key, f.Value)))
                .GetJsonAsync<TZohoPurchaseOrderList>();

        public Task<ZohoPurchaseOrder> GetAsync(string id) => GetAsync<ZohoPurchaseOrder>(id);
        
        public Task<TZohoPurchaseOrder> GetAsync<TZohoPurchaseOrder>(string id) where TZohoPurchaseOrder : ZohoPurchaseOrder =>
            Get(id).GetJsonAsync<TZohoPurchaseOrder>();
        
        public Task<ZohoPurchaseOrder> SaveAsync(ZohoPurchaseOrder purchaseOrder) => SaveAsync<ZohoPurchaseOrder>(purchaseOrder);

        public async Task<TZohoPurchaseOrder> SaveAsync<TZohoPurchaseOrder>(TZohoPurchaseOrder purchaseOrder)
            where TZohoPurchaseOrder : ZohoPurchaseOrder => 
            await Put<TZohoPurchaseOrder>(purchaseOrder, purchaseOrder.purchaseorder_id);
            
        public Task<ZohoPurchaseOrder> CreateAsync(ZohoPurchaseOrder purchaseOrder) => CreateAsync<ZohoPurchaseOrder>(purchaseOrder);

        public async Task<TZohoPurchaseOrder> CreateAsync<TZohoPurchaseOrder>(TZohoPurchaseOrder purchaseOrder)
            where TZohoPurchaseOrder : ZohoPurchaseOrder =>
            await Post<TZohoPurchaseOrder>(purchaseOrder);

        public Task DeleteAsync(string id) => Delete(id).DeleteAsync();
    }
}
