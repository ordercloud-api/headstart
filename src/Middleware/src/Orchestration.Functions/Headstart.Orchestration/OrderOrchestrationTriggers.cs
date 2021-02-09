using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using Headstart.Common.Queries;
using Headstart.Common;
using System.Threading.Tasks;
using System.Linq;
using Headstart.Common.Helpers;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Headstart.API.Commands;

namespace Headstart.Orchestration
{
    public class OrderOrchestrationTrigger
    {
        private readonly LogQuery _log;
        private readonly AppSettings _appSettings;
        private readonly IProductUpdateCommand _productUpdateCommand;
       

        public OrderOrchestrationTrigger(AppSettings appSettings, IOrderCloudIntegrationsFunctionToken token, 
            ISupplierSyncCommand supplier, LogQuery log, IProductUpdateCommand productUpdateCommand)
        {
            _productUpdateCommand = productUpdateCommand;
            _log = log;
            _appSettings = appSettings;
        }

        [FunctionName("EmailProductUpdates")]
        public async Task EmailProductUpdates([TimerTrigger("0 15 9 * * *")] TimerInfo myTimer, [OrchestrationClient] DurableOrchestrationClient client, ILogger logger)
        {
            // run every day at 9:15am
            logger.LogInformation("Starting function");
            try
            {
                await _productUpdateCommand.SendAllProductUpdateEmails();
                await _productUpdateCommand.CleanUpProductHistoryData();
            }
            catch (Exception ex)
            {
                logger.LogError(ex.Message);
            }
        }
    }
}
