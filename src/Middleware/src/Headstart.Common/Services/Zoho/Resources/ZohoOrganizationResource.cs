using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using Headstart.Common.Services.Zoho.Models;

namespace Headstart.Common.Services.Zoho.Resources
{
    public interface IZohoOrganizationResource
    {
        Task<ZohoOrganizationList> ListAsync(params ZohoFilter[] filters);
        Task<TZohoOrganizationList> ListAsync<TZohoOrganizationList>(params ZohoFilter[] filters) where TZohoOrganizationList : ZohoOrganizationList;
        Task<ZohoOrganization> GetAsync(string id);
        Task<TZohoOrganization> GetAsync<TZohoOrganization>(string id) where TZohoOrganization : ZohoOrganization;
        Task<ZohoOrganization> SaveAsync(ZohoOrganization org);
        Task<TZohoOrganization> SaveAsync<TZohoOrganization>(TZohoOrganization org) where TZohoOrganization : ZohoOrganization;
        Task<ZohoOrganization> CreateAsync(ZohoOrganization org);
        Task<TZohoOrganization> CreateAsync<TZohoOrganization>(TZohoOrganization org) where TZohoOrganization : ZohoOrganization;
        Task DeleteAsync(string id);
    }

    public class ZohoOrganizationResource : ZohoResource, IZohoOrganizationResource
    {
        internal ZohoOrganizationResource(ZohoClient client) : base(client, "organization", "organizations") { }

        public Task<ZohoOrganizationList> ListAsync(params ZohoFilter[] filters) => ListAsync<ZohoOrganizationList>(filters);
        public Task<TZohoOrganizationList> ListAsync<TZohoOrganizationList>(params ZohoFilter[] filters) where TZohoOrganizationList : ZohoOrganizationList => Get()
                .SetQueryParams(filters?.Select(f => new KeyValuePair<string, object>(f.Key, f.Value)))
                .GetJsonAsync<TZohoOrganizationList>();

        public Task<ZohoOrganization> GetAsync(string id) => GetAsync<ZohoOrganization>(id);
        
        public Task<TZohoOrganization> GetAsync<TZohoOrganization>(string id) where TZohoOrganization : ZohoOrganization =>
            Get(id).GetJsonAsync<TZohoOrganization>();
        
        public Task<ZohoOrganization> SaveAsync(ZohoOrganization org) => SaveAsync<ZohoOrganization>(org);

        public async Task<TZohoOrganization> SaveAsync<TZohoOrganization>(TZohoOrganization org)
            where TZohoOrganization : ZohoOrganization => 
            await Put<TZohoOrganization>(org, org.organization_id);
            
        public Task<ZohoOrganization> CreateAsync(ZohoOrganization org) => CreateAsync<ZohoOrganization>(org);

        public async Task<TZohoOrganization> CreateAsync<TZohoOrganization>(TZohoOrganization org)
            where TZohoOrganization : ZohoOrganization =>
            await Post<TZohoOrganization>(org);

        public Task DeleteAsync(string id) => Delete(id).DeleteAsync();
    }
}
