﻿using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Library.Cosmos;

namespace OrderCloud.Integrations.Library
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum SortDirection
    {
        ASC,
        DESC,
    }

    public interface IRepository<T> where T : CosmosObject
    {
        Task<CosmosListPage<T>> GetItemsAsync(IQueryable<T> queryable, QueryRequestOptions requestOptions, CosmosListOptions listOptions);

        Task<T> GetItemAsync(string id);

        Task<T> AddItemAsync(T item);

        Task UpsertItemAsync(string id, T item);

        Task<ItemResponse<T>> ReplaceItemAsync(string id, T item);

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

        public IList<ListFilter> Filters { get; set; } = new List<ListFilter>();

        public string Sort { get; set; }

        public SortDirection? SortDirection { get; set; }

        public string Search { get; set; }

        public string SearchOn { get; set; }
    }
}
