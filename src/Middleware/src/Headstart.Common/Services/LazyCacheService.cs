using System;
using LazyCache;
using OrderCloud.Catalyst;
using System.Threading.Tasks;

namespace OrderCloud.Common.Services
{
	/// Lazy Cache (https://github.com/alastairtree/LazyCache) is a local in-memory cache. 
	/// It requires no setup, but is likely not appropriate for a server with multiple instances. 
	/// Each instance would have a separate cache, and behavior would be inconsient.
	public class LazyCacheService : ISimpleCache
	{
		private readonly IAppCache _cache = new CachingService();

		public async Task<T> GetOrAddAsync<T>(string key, TimeSpan expireAfter, Func<Task<T>> addItemFactory)
		{
			return await _cache.GetOrAddAsync(key, addItemFactory, expireAfter);
		}

		public async Task RemoveAsync(string key)
		{
			_cache.Remove(key);
		}
	}
}