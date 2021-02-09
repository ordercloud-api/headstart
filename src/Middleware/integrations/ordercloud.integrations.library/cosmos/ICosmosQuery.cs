using System.Collections.Generic;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace ordercloud.integrations.library
{
    public interface ICosmosQuery<T> where T : ICosmosObject
    {
        Task<ListPage<T>> List(IListArgs args);
        Task<T> Get(string id);
        Task<T> Save(T entity);
        Task<List<T>> SaveMany(List<T> entities);
        Task Delete(string id);
    }

    public interface IPartitionedCosmosQuery<T> where T : ICosmosObject
    {
        Task<ListPage<T>> List(IListArgs args, string partitionKey);
        Task<T> Get(string id, string partitionKey);
        Task<T> Save(T entity, string partitionKey);
        Task<List<T>> SaveMany(List<T> entities, string partitionKey);
        Task Delete(string id, string partitionKey);
    }
}
