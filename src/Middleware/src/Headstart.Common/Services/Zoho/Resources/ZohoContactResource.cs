using Flurl.Http;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
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
		internal ZohoContactResource(ZohoClient client) : base(client, @"contact", @"contacts")
		{
		}

		public Task<ZohoContactList> ListAsync(params ZohoFilter[] filters)
		{
			return ListAsync<ZohoContactList>(filters);
		}

		public Task<TZohoContactList> ListAsync<TZohoContactList>(params ZohoFilter[] filters) where TZohoContactList : ZohoContactList
		{
			return Get().SetQueryParams(filters?.Select(f => new KeyValuePair<string, object>(f.Key, f.Value))).GetJsonAsync<TZohoContactList>();
		}

		public Task<ZohoSingleContact> GetAsync(string id)
		{
			return GetAsync<ZohoSingleContact>(id);
		}

		public Task<TZohoContact> GetAsync<TZohoContact>(string id) where TZohoContact : ZohoSingleContact
		{
			return Get(id).GetJsonAsync<TZohoContact>();
		}

		public Task<ZohoContact> SaveAsync(ZohoContact contact)
		{
			return SaveAsync<ZohoContact>(contact);
		}

		public async Task<TZohoContact> SaveAsync<TZohoContact>(TZohoContact contact) where TZohoContact : ZohoContact
		{
			return await Put<TZohoContact>(contact, contact.contact_id);
		}

		public Task<ZohoContact> CreateAsync(ZohoContact contact)
		{
			return CreateAsync<ZohoContact>(contact);
		}

		public async Task<TZohoContact> CreateAsync<TZohoContact>(TZohoContact contact) where TZohoContact : ZohoContact
		{
			return await Post<TZohoContact>(contact);
		}

		public Task DeleteAsync(string id)
		{
			return Delete(id).DeleteAsync();
		}
	}
}