using System;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json.Linq;
using Headstart.Common.Exceptions;
using Headstart.Common.Helpers;
using Headstart.Common.Models;
using Headstart.Common.Queries;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using Action = Headstart.Common.Models.Action;
using LogLevel = Headstart.Common.Models.LogLevel;

namespace Headstart.Orchestration
{
    public class OrchestrationWorkflow
    {
        private readonly IOrchestrationCommand _orch;
        private readonly ISyncCommand _sync;
        private readonly IOrderCloudIntegrationsFunctionToken _token;
        private readonly LogQuery _log;

        public OrchestrationWorkflow(IOrchestrationCommand orch, ISyncCommand sync, IOrderCloudIntegrationsFunctionToken token, LogQuery log)
        {
            _orch = orch;
            _sync = sync;
            _log = log;
            _token = token;
        }

        [FunctionName("OrchestrationWorkflow")]
        public async Task RunOrchestrator([OrchestrationTrigger] DurableOrchestrationContext context, ILogger log)
        {
            try
            {
                var path = context.GetInput<string>();

                var wi = new WorkItem(path)
                {
                    Cache = await context.CallActivityAsync<JObject>("GetCachedItem", path),
                };
                var queue = await context.CallActivityAsync<JObject>("GetQueuedItem", path);
                wi.Current = queue["Model"] as JObject;
                wi.Token = queue["Token"].ToString();
                wi.ClientId = queue["ClientId"].ToString();
                wi.Diff = await context.CallActivityAsync<JObject>("CalculateDiff", wi);
                wi.Action = await context.CallActivityAsync<Action>("DetermineAction", wi);

                switch (wi.Action)
                {
                    case Action.Ignore:
                        wi.Cache = null;
                        break;
                    case Action.Get:
                        wi.Cache = await context.CallActivityAsync<JObject>("RefreshCache", wi);
                        break;
                    case Action.Create:
                    case Action.Delete:
                    case Action.Patch:
                    case Action.Update:
                    default:
                        wi.Cache = await context.CallActivityAsync<JObject>("OrderCloudAction", wi);
                        break;
                }
                
                await context.CallActivityAsync("UpdateCache", wi);

                log.LogDebug($"{wi.RecordId}: {wi.Action} success");
                log.LogInformation($"{wi.RecordId}: {wi.Action} successfully");
                await context.CallActivityAsync<JObject>("LogEvent", new OrchestrationLog(wi) { Level = LogLevel.Success });
            }
            catch (OrchestrationException oex)
            {
                log.LogError($"{oex.Error.Type}: {oex.Message}", oex.Error.Data);
                log.LogDebug($"{oex.Error.Type}: {oex.Message}", oex.Error.Data);
            }
            catch (FunctionFailedException fex)
            {
                log.LogError(fex.Message);
                log.LogDebug(fex.Message);
            }
            catch (Exception ex)
            {
                log.LogError(ex.Message);
                log.LogDebug(ex.Message);
            }
            finally
            {
                await context.CallActivityAsync("CleanupQueue", context.GetInput<string>());
            }
        }

        [FunctionName("DefineUserContext")]
        public async Task<VerifiedUserContext> DefineUserContext([ActivityTrigger] WorkItem wi) => await _token.Authorize(wi.Token, new[] { ApiRole.ProductAdmin, ApiRole.PriceScheduleAdmin });

        [FunctionName("LogEvent")]
        public async Task LogEvent([ActivityTrigger] OrchestrationLog log) => await _log.Save(log);

        [FunctionName("RefreshCache")]
        public async Task<JObject> RefreshCache([ActivityTrigger] WorkItem wi) => await _sync.Dispatch(wi);

        [FunctionName("CleanupQueue")]
        public async Task CleanupQueue([ActivityTrigger] string path) => await _orch.CleanupQueue(path);

        [FunctionName("UpdateCache")]
        public async Task UpdateCache([ActivityTrigger] WorkItem wi) => await _orch.SaveToCache(wi);

        [FunctionName("OrderCloudAction")]
        public async Task<JObject> OrderCloudAction([ActivityTrigger] WorkItem wi) => await _sync.Dispatch(wi);

        [FunctionName("DetermineAction")]
        public async Task<Action> DetermineAction([ActivityTrigger] WorkItem wi) => await _orch.DetermineAction(wi);

        [FunctionName("CalculateDiff")]
        public async Task<JObject> CalculateDiff([ActivityTrigger] WorkItem wi) => await _orch.CalculateDiff(wi);

        [FunctionName("GetQueuedItem")]
        public async Task<JObject> GetQueuedItem([ActivityTrigger] string path) => await _orch.GetQueuedItem(path);

        [FunctionName("GetCachedItem")]
        public async Task<JObject> GetCachedItem([ActivityTrigger] string path) => await _orch.GetCachedItem(path);
    }
}