using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Cosmonaut;
using Cosmonaut.Extensions;
using Headstart.Common.Models;
using Microsoft.Azure.Documents.Client;
using OrderCloud.Integrations.Library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using OrderCloud.Integrations.Library.Cosmos;

namespace Headstart.Common.Queries
{
	public class LogQuery : ICosmosQuery<OrchestrationLog>
	{
		private readonly ICosmosStore<OrchestrationLog> store;

		public LogQuery(ICosmosStore<OrchestrationLog> store)
		{
			this.store = store;
		}

		public async Task<ListPage<OrchestrationLog>> List(IListArgs args)
		{
			var query = store.Query(new FeedOptions() { EnableCrossPartitionQuery = true })
				.Search(args)
				.Filter(args)
				.Sort(args);
			var list = await query.WithPagination(args.Page, args.PageSize).ToPagedListAsync();
			var count = await query.CountAsync();
			return list.ToListPage(args.Page, args.PageSize, count);
		}

		public async Task<OrchestrationLog> Get(string id)
		{
			var item = await store.FindAsync(id);
			return item;
		}

		public async Task<OrchestrationLog> Save(OrchestrationLog log)
		{
			log.timeStamp = DateTime.Now;
			var result = await store.UpsertAsync(log);
			return result.Entity;
		}

		public async Task<List<OrchestrationLog>> SaveMany(List<OrchestrationLog> logs)
		{
			var result = await store.UpsertRangeAsync(logs);
			return result.SuccessfulEntities.Select(e => e.Entity).ToList();
		}

		public async Task Delete(string id)
		{
			await store.RemoveByIdAsync(id);
		}
	}
}
