using System.Threading.Tasks;
using Headstart.Common;
using Headstart.Common.Models;
using Headstart.Common.Queries;
using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace Headstart.Orchestration
{
    public interface IOrchestrationLogCommand
    {
        Task<ListPage<OrchestrationLog>> List(ListArgs<OrchestrationLog> hsListArgs);
    }

    public class OrchestrationLogCommand : IOrchestrationLogCommand
    {
        private readonly AppSettings _settings;
        private readonly LogQuery _log;

        public OrchestrationLogCommand(AppSettings settings, LogQuery log)
        {
            _settings = settings;
            _log = log;
        }

        public async Task<ListPage<OrchestrationLog>> List(ListArgs<OrchestrationLog> hsListArgs)
        {
            var logs = await _log.List(hsListArgs);
            return logs;
        }
    }
}
