using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Headstart.Common.Services.Zoho.Models;

namespace Headstart.Common.Services.Zoho.Resources
{
    public interface IZohoContactResource
    {
        Task<ZohoContactList> ListAsync(params ZohoFilter[] filters);
        Task<TZohoContactList> ListAsync<TZohoContactList>(params ZohoFilter[] filters) where TZohoContactList : ZohoContactList;
        Task<ZohoSingleContact> GetAsync(string id);
        Task<TZohoContact> GetAsync<TZohoContact>(string id) where TZohoContact : ZohoSingleContact;
        Task<ZohoContact> SaveAsync(ZohoContact contact);
        Task<TZohoContact> SaveAsync<TZohoContact>(TZohoContact contact) where TZohoContact : ZohoContact;
        Task<ZohoContact> CreateAsync(ZohoContact contact);
        Task<TZohoContact> CreateAsync<TZohoContact>(TZohoContact contact) where TZohoContact : ZohoContact;
        Task DeleteAsync(string id);
    }

    public class ZohoContactResource : ZohoResource, IZohoContactResource
    {
        internal ZohoContactResource(ZohoClient client) : base(client, "contact", "contacts") { }

        public Task<ZohoContactList> ListAsync(params ZohoFilter[] filters) => ListAsync<ZohoContactList>(filters);
        public Task<TZohoContactList> ListAsync<TZohoContactList>(params ZohoFilter[] filters) where TZohoContactList : ZohoContactList => Get()
                .SetQueryParams(filters?.Select(f => new KeyValuePair<string, object>(f.Key, f.Value)))
                .GetJsonAsync<TZohoContactList>();

        public Task<ZohoSingleContact> GetAsync(string id) => GetAsync<ZohoSingleContact>(id);
        
        public Task<TZohoContact> GetAsync<TZohoContact>(string id) where TZohoContact : ZohoSingleContact =>
            Get(id).GetJsonAsync<TZohoContact>();
        
        public Task<ZohoContact> SaveAsync(ZohoContact contact) => SaveAsync<ZohoContact>(contact);

        public async Task<TZohoContact> SaveAsync<TZohoContact>(TZohoContact contact)
            where TZohoContact : ZohoContact => 
            await Put<TZohoContact>(contact, contact.contact_id);
            
        public Task<ZohoContact> CreateAsync(ZohoContact contact) => CreateAsync<ZohoContact>(contact);

        public async Task<TZohoContact> CreateAsync<TZohoContact>(TZohoContact contact)
            where TZohoContact : ZohoContact =>
            await Post<TZohoContact>(contact);

        public Task DeleteAsync(string id) => Delete(id).DeleteAsync();
    }
}
