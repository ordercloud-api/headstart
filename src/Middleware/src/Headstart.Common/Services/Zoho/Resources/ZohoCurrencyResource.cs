using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Headstart.Common.Services.Zoho.Models;

namespace Headstart.Common.Services.Zoho.Resources
{
    public interface IZohoCurrencyResource
    {
        Task<ZohoListCurrencyList> ListAsync(params ZohoFilter[] filters);
        Task<TZohoCurrencyList> ListAsync<TZohoCurrencyList>(params ZohoFilter[] filters) where TZohoCurrencyList : ZohoListCurrencyList;
        Task<ZohoCurrency> GetAsync(string id);
        Task<TZohoCurrency> GetAsync<TZohoCurrency>(string id) where TZohoCurrency : ZohoCurrency;
        Task<ZohoCurrency> SaveAsync(ZohoCurrency currency);
        Task<TZohoCurrency> SaveAsync<TZohoCurrency>(TZohoCurrency currency) where TZohoCurrency : ZohoCurrency;
        Task<ZohoCurrency> CreateAsync(ZohoCurrency contact);
        Task<TZohoCurrency> CreateAsync<TZohoCurrency>(TZohoCurrency contact) where TZohoCurrency : ZohoCurrency;
        Task DeleteAsync(string id);
    }

    public class ZohoCurrencyResource : ZohoResource, IZohoCurrencyResource
    {
        internal ZohoCurrencyResource(ZohoClient client) : base(client, "currency", "settings", "currencies") { }

        public Task<ZohoListCurrencyList> ListAsync(params ZohoFilter[] filters) => ListAsync<ZohoListCurrencyList>(filters);
        public Task<TZohoCurrencyList> ListAsync<TZohoCurrencyList>(params ZohoFilter[] filters) where TZohoCurrencyList : ZohoListCurrencyList
        {
            var queryParams = filters?.Select(f => new KeyValuePair<string, object>(f.Key, f.Value));
            return Get()
                .SetQueryParams(queryParams)
                .GetJsonAsync<TZohoCurrencyList>();
        }

        public Task<ZohoCurrency> GetAsync(string id) => GetAsync<ZohoCurrency>(id);
        
        public Task<TZohoCurrency> GetAsync<TZohoCurrency>(string id) where TZohoCurrency : ZohoCurrency =>
            Get(id).GetJsonAsync<TZohoCurrency>();
        
        public Task<ZohoCurrency> SaveAsync(ZohoCurrency currency) => SaveAsync<ZohoCurrency>(currency);
        public Task<TZohoCurrency> SaveAsync<TZohoCurrency>(TZohoCurrency currency) where TZohoCurrency : ZohoCurrency =>
        
            Get(currency.currency_id).PutJsonAsync(currency).ReceiveJson<TZohoCurrency>();
        
        public Task<ZohoCurrency> CreateAsync(ZohoCurrency contact) => CreateAsync<ZohoCurrency>(contact);
        
        public Task<TZohoCurrency> CreateAsync<TZohoCurrency>(TZohoCurrency contact) where TZohoCurrency : ZohoCurrency =>
            Get().PostJsonAsync(contact).ReceiveJson<TZohoCurrency>();
        
        public Task DeleteAsync(string id) => Get(id).DeleteAsync();
    }
}
