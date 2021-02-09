using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;

namespace ordercloud.integrations.library 
{ 
    public abstract class CosmosDbRepository<T> : IRepository<T>, IContainerContext<T> where T : CosmosObject
    {
        public abstract string ContainerName { get; }
        public abstract PartitionKey ResolvePartitionKey(string entityId);
        private readonly ICosmosDbContainerFactory _cosmosDbContainerFactory;
        private readonly Container _container;
        public CosmosDbRepository(ICosmosDbContainerFactory cosmosDbContainerFactory)
        {
            _cosmosDbContainerFactory = cosmosDbContainerFactory ?? throw new ArgumentNullException(nameof(ICosmosDbContainerFactory));
            _container = _cosmosDbContainerFactory.GetContainer(ContainerName)._container;
        }

        public async Task<T> AddItemAsync(T item)
        {
            return await _container.CreateItemAsync<T>(item, ResolvePartitionKey(item.id));
        }

        public async Task DeleteItemAsync(string id)
        {
            await _container.DeleteItemAsync<T>(id, ResolvePartitionKey(id));
        }

        public async Task<T> GetItemAsync(string id)
        {
            try
            {
                ItemResponse<T> response = await _container.ReadItemAsync<T>(id, ResolvePartitionKey(id));
                return response.Resource;
            }
            catch (CosmosException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }
        }

        public async Task<CosmosListPage<T>> GetItemsAsync(IQueryable<T> queryable, QueryRequestOptions requestOptions, string continuationToken = null)
        {
            QueryDefinition queryDefinition = queryable.ToQueryDefinition();
            FeedIterator<T> queryResultSetIterator = _container.GetItemQueryIterator<T>(queryDefinition, continuationToken, requestOptions);
            
            List<T> results = new List<T>();
            FeedResponse<T> currentResultSet = await queryResultSetIterator.ReadNextAsync();
            foreach (T document in currentResultSet)
            {
                results.Add(document);
            }

            Response<int> count = await queryable.CountAsync();

            return new CosmosListPage<T>() {
                Meta = new CosmosMeta() { PageSize = (int)requestOptions.MaxItemCount, Total = count.Resource, ContinuationToken = currentResultSet.ContinuationToken },
                Items = results
            };
        }

        public async Task UpdateItemAsync(string id, T item)
        {
            await _container.UpsertItemAsync<T>(item, ResolvePartitionKey(id));
        }

        public IQueryable<T> GetQueryable()
        {
            return _container.GetItemLinqQueryable<T>();
        }
    }
}
