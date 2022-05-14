using OrderCloud.Catalyst;
using System;
using System.Threading.Tasks;
using LazyCache;

namespace OrderCloud.Common.Services
{
	// Lazy Cache (https://github.com/alastairtree/LazyCache) is a local in-memory cache.
	// It requires no setup, but is likely not appropriate for a server with multiple instances.
	// Each instance would have a separate cache, and behavior would be inconsient.
	public class LazyCacheService : ISimpleCache
	{
		private readonly IAppCache cache = new CachingService();

		public async Task<T> GetOrAddAsync<T>(string key, TimeSpan expireAfter, Func<Task<T>> addItemFactory)
			=> await cache.GetOrAddAsync(key, addItemFactory, expireAfter);

		public async Task RemoveAsync(string key)
		{
			cache.Remove(key);
			await Task.CompletedTask;
		}
	}
}
