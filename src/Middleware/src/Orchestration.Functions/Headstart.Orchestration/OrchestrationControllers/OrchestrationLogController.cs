using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Headstart.Common.Models;
using Headstart.Models.Attributes;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using Headstart.API.Controllers;
using Headstart.Common;

namespace Headstart.Orchestration
{
    [DocComments("\"Orchestration Logs\" represents logs of orchestration activities")]
    [HSSection.Orchestration(ListOrder = 3)]
    [Route("orchestration/logs")]
    public class OrchestrationLogController : BaseController
    {
        private readonly IOrchestrationLogCommand _command;
        public OrchestrationLogController(AppSettings settings, IOrchestrationLogCommand command) : base(settings)
        {
            _command = command;
        }

        [DocName("GET Orchestration Logs")]
        [HttpGet]
        public async Task<ListPage<OrchestrationLog>> List(ListArgs<OrchestrationLog> hsListArgs)
        {
            return await _command.List(hsListArgs);
        }
    }
}
