using Microsoft.Azure.Cosmos;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ordercloud.integrations.library
{
    public interface IRepository<T> where T : CosmosObject
    {
        Task<CosmosListPage<T>> GetItemsAsync(IQueryable<T> queryable, QueryRequestOptions requestOptions, string continuationToken = null);
        Task<T> GetItemAsync(string id);
        Task<T> AddItemAsync(T item);
        Task UpdateItemAsync(string id, T item);
        Task DeleteItemAsync(string id);
        IQueryable<T> GetQueryable();
    }

    public class CosmosListPage<T>
    {
        public CosmosMeta Meta { get; set; }
        public List<T> Items { get; set; }
    }

    public class CosmosMeta
    {
        public int PageSize { get; set; }
        public int Total { get; set; }
        public string ContinuationToken { get; set; }
    }

    public class CosmosListOptions
    {
        public int PageSize { get; set; }
        public string ContinuationToken { get; set; }
        // TO-DO - Filters, SortBy, SearchOn
    }
}