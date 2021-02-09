using Cosmonaut;
using Cosmonaut.Extensions;
using Headstart.Common.Models;
using Microsoft.Azure.Documents;
using Microsoft.Azure.Documents.Client;
using ordercloud.integrations.library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Headstart.Common.Queries
{
    public interface IResourceHistoryQuery<T> where T : class
    {
        Task Delete(string cosmosID);
        Task<List<T>> List(string resourceID);
        Task<List<T>> ListByDate(DateTime date);
        Task<T> Post(T update);
        Task<T> Put(T update);
        Task<T> GetVersionByDate(string resourceID, DateTime date);
        Task DeleteMany(List<string> cosmosIDs);
    }

    public class ResourceHistoryQuery<T> : IResourceHistoryQuery<T> where T : class, IResourceHistory
    {
        private readonly ICosmosStore<T> _productStore;
        public ResourceHistoryQuery(ICosmosStore<T> productStore)
        {
            _productStore = productStore;
        }

        public async Task<List<T>> List(string resourceID)
        {
            // list product history given product ID. TODO: get the most RECENT record.
            var feedOptions = new FeedOptions() { PartitionKey = new PartitionKey(resourceID) };
            return await _productStore.Query(feedOptions).ToListAsync();
        }

        public async Task<T> Post(T update)
        {
            var time = DateTime.Now;
            update.DateLastUpdated = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
            var previousResource = (await List(update.ResourceID)).Where(record => record.ResourceID == update.ResourceID);
            if(previousResource.Count()>0)
            {
                await DeleteMany(previousResource.Select(r => r.id).ToList());
            }
            var newProductUpdate = await _productStore.AddAsync(update);
            return newProductUpdate;
        }

        public async Task<T> Put(T update)
        {
            var time = DateTime.Now;
            update.DateLastUpdated = new DateTime(time.Year, time.Month, time.Day, 0, 0, 0);
            var resourceToUpdate = (await List(update.ResourceID)).FirstOrDefault(record => record.ResourceID == update.ResourceID && record.DateLastUpdated == update.DateLastUpdated);
            try
            {
                update.id = resourceToUpdate.id;
                update.Action = resourceToUpdate.Action;
                return await _productStore.UpdateAsync(update);
            }
            catch
            {
                return await _productStore.AddAsync(update);
            }
        }

        public async Task<List<T>> ListByDate(DateTime date)
        {
            var start = new DateTime(date.Year, date.Month, date.Day, 0, 0, 0);
            var nextDay = date.AddDays(1);
            var end = new DateTime(nextDay.Year, nextDay.Month, nextDay.Day, 0, 0, 0);
            var resource = await _productStore.Query()
                .Where(resource => resource.DateLastUpdated >= start && resource.DateLastUpdated < end).ToListAsync();
            return resource;
        }

        public async Task<T> GetVersionByDate(string resourceID, DateTime date)
        {
            if (resourceID == null) return null;
            var updates = (await List(resourceID))?.Where(p => p.DateLastUpdated < date);
            var maxDate = updates?.Max(prop => prop?.DateLastUpdated);
            return updates.Where(p => p?.DateLastUpdated == maxDate)?.FirstOrDefault();

        }

        public async Task Delete(string cosmosID)
        {
            await _productStore.RemoveAsync(resource => resource.id == cosmosID);
        }

        public async Task DeleteMany(List<string> cosmosIDs)
        {
            await Throttler.RunAsync(cosmosIDs, 100, 5, async id =>
            {
                await _productStore.RemoveAsync(resource => resource.id == id);
            });
        }
    }
}
