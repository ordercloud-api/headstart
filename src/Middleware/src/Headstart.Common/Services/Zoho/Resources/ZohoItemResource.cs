using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Flurl.Http;
using Headstart.Common.Services.Zoho.Models;

namespace Headstart.Common.Services.Zoho.Resources
{
    public interface IZohoItemResource
    {
        Task<ZohoItemList> ListAsync(params ZohoFilter[] filters);
        Task<TZohoItemList> ListAsync<TZohoItemList>(params ZohoFilter[] filters) where TZohoItemList : ZohoItemList;
        //Task<ZohoLineItem> GetAsync(string id);
        //Task<TZohoItem> GetAsync<TZohoItem>(string id) where TZohoItem : ZohoLineItem;
        Task<ZohoLineItem> SaveAsync(ZohoLineItem item);
        Task<TZohoItem> SaveAsync<TZohoItem>(TZohoItem item) where TZohoItem : ZohoLineItem;
        Task<ZohoLineItem> CreateAsync(ZohoLineItem item);
        Task<TZohoItem> CreateAsync<TZohoItem>(TZohoItem item) where TZohoItem : ZohoLineItem;
        Task DeleteAsync(string id);
        Task MarkActiveAsync(string id);
    }

    public class ZohoItemResource : ZohoResource, IZohoItemResource
    {
        internal ZohoItemResource(ZohoClient client) : base(client, "item", "items") { }

        public Task<ZohoItemList> ListAsync(params ZohoFilter[] filters) => ListAsync<ZohoItemList>(filters);
        public Task<TZohoItemList> ListAsync<TZohoItemList>(params ZohoFilter[] filters) where TZohoItemList : ZohoItemList => Get()
                .SetQueryParams(filters?.Select(f => new KeyValuePair<string, object>(f.Key, f.Value)))
                .GetJsonAsync<TZohoItemList>();

        //public Task<ZohoLineItem> GetAsync(string id) => GetAsync<ZohoLineItem>(id);
        
        //public Task<TZohoItem> GetAsync<TZohoItem>(string id) where TZohoItem : ZohoLineItem =>
        //    Get(id).GetJsonAsync<TZohoItem>();
        
        public Task<ZohoLineItem> SaveAsync(ZohoLineItem item) => SaveAsync<ZohoLineItem>(item);

        public async Task<TZohoItem> SaveAsync<TZohoItem>(TZohoItem item)
            where TZohoItem : ZohoLineItem => 
            await Put<TZohoItem>(item, item.item_id);
            
        public Task<ZohoLineItem> CreateAsync(ZohoLineItem item) => CreateAsync<ZohoLineItem>(item);

        public async Task<TZohoItem> CreateAsync<TZohoItem>(TZohoItem item)
            where TZohoItem : ZohoLineItem =>
            await Post<TZohoItem>(item);

        public Task DeleteAsync(string id) => Delete(id).DeleteAsync();

        public async Task MarkActiveAsync(string id)
        {
            await Post(id, "active").SendAsync(HttpMethod.Post);
        }
    }
}
