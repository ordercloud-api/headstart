using System;
using System.Threading.Tasks;
using Headstart.Common.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Headstart.Common.Extensions;
using Headstart.Common.Models;
using Headstart.Common.Queries;
using Headstart.Models;
using ordercloud.integrations.library;
using Action = Headstart.Common.Models.Action;
using Headstart.Common;

namespace Headstart.Orchestration
{
    public interface IOrchestrationCommand
    {
        Task CleanupQueue(string path);
        Task SaveToCache(WorkItem wi);
        Task<Action> DetermineAction(WorkItem wi);
        Task<JObject> CalculateDiff(WorkItem wi);
        Task<JObject> GetQueuedItem(string path);
        Task<JObject> GetCachedItem(string path);
        Task<T> SaveToQueue<T>(T obj, VerifiedUserContext user, string resourceId) where T : IHSObject;
    }

    public class OrchestrationCommand : IOrchestrationCommand
    {
        private readonly IOrderCloudIntegrationsBlobService _blobQueue;
        private readonly IOrderCloudIntegrationsBlobService _blobCache;
        private readonly AppSettings _settings;
        private readonly LogQuery _log;

        public OrchestrationCommand(AppSettings settings, LogQuery log)
        {
            _settings = settings;
            _blobQueue = new OrderCloudIntegrationsBlobService(new BlobServiceConfig()
            {
#if DEBUG
                ConnectionString = "UseDevelopmentStorage=true",
#else
                ConnectionString = settings.BlobSettings.ConnectionString,
#endif
                Container = settings.BlobSettings.ContainerNameQueue
            });

            _blobCache = new OrderCloudIntegrationsBlobService(new BlobServiceConfig()
            {
#if DEBUG
                ConnectionString = "UseDevelopmentStorage=true",
#else
                ConnectionString = settings.BlobSettings.ConnectionString,
#endif
                Container = settings.BlobSettings.ContainerNameCache
            });
            _log = log;
        }

        public async Task<T> SaveToQueue<T>(T obj, VerifiedUserContext user, string resourceId) where T : IHSObject
        {
            try
            {
                var orch = new OrchestrationObject<T>
                {
                    Token = user.AccessToken,
                    ClientId = user.ClientID,
                    ID = obj.ID,
                    Model = obj
                };
                await _blobQueue.Save(orch.BuildPath(resourceId, user.ClientID), JsonConvert.SerializeObject(orch));
                return await Task.FromResult(obj);
            }
            catch (OrderCloudIntegrationException ex)
            {
                throw new OrderCloudIntegrationException(ex.ApiError);
            }
            catch (Exception ex)
            {
                await _log.Save(new OrchestrationLog()
                {
                    Level = LogLevel.Error,
                    Message = $"Failed to save blob to queue from API: {user.SupplierID} - {typeof(T)}:  {ex.Message}",
                    Current = JObject.FromObject(obj)
                });
                throw new OrderCloudIntegrationException(Headstart.Models.ErrorCodes.All["WriteFailure"], obj);
            }
        }

        public async Task CleanupQueue(string path)
        {
            try
            {
                await _blobQueue.Delete(path);
            }
            catch (Exception ex)
            {
                await _log.Save(new OrchestrationLog()
                {
                    Level = LogLevel.Error,
                    Message = $"Failed to remove blob to queue: {path}",
                    
                });
                throw new OrchestrationException(OrchestrationErrorType.QueueCleanupError, $"{path} | {ex.Message}");
            }
        }
        public async Task SaveToCache(WorkItem wi)
        {
            if (wi.Cache == null) await Task.CompletedTask;
            try
            {
                await _blobCache.Save($"{wi.ResourceId.ToLower()}/{wi.ClientId}/{wi.RecordType.ToString().ToLower()}/{wi.RecordId}", wi.Cache);
            }
            catch (Exception ex)
            {
                throw new OrchestrationException(OrchestrationErrorType.CacheUpdateError, wi, ex.Message);
            }
        }

        public async Task<JObject> GetQueuedItem(string path)
        {
            try
            {
                var blob = await _blobQueue.Get<JObject>(path);
                return blob;
            }
            catch (Exception ex)
            {
                throw new OrchestrationException(OrchestrationErrorType.QueuedGetError, $"{path} | {ex.Message}");
            }
        }

        public async Task<JObject> GetCachedItem(string path)
        {
            try
            {
                var blob = await _blobCache.Get<JObject>($"{path}");
                return blob;
            }
            catch (Exception)
            {
                return null; // we don't want to error here. there is an expectation that new items won't be in cache
            }
        }

        public async Task<JObject> CalculateDiff(WorkItem wi)
        {
            try
            {
                return await Task.FromResult(wi.Current.Diff(wi.Cache));
            }
            catch (Exception ex)
            {
                throw new OrchestrationException(OrchestrationErrorType.DiffCalculationError, wi, ex.Message);
            }
        }

        public async Task<Action> DetermineAction(WorkItem wi)
        {
            try
            {
                var action = WorkItemMethods.DetermineAction(wi);
                return await Task.FromResult(action);
            }
            catch (Exception ex)
            {
                throw new OrchestrationException(OrchestrationErrorType.ActionEvaluationError, wi, ex.Message);
            }
        }
    }
}
