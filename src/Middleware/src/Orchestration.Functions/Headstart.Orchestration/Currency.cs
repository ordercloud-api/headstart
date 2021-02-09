using Flurl.Http.Configuration;
using Headstart.Common;
using LazyCache;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using ordercloud.integrations.exchangerates;
using ordercloud.integrations.library;
using System.Threading.Tasks;

namespace Headstart.Orchestration
{
    public class Currency
    {
        private readonly IExchangeRatesCommand _command;
        public Currency(AppSettings settings, IFlurlClientFactory flurlFactory, IAppCache cache)
        {
            _command = new ExchangeRatesCommand(new BlobServiceConfig()
            {
                ConnectionString = settings.BlobSettings.ConnectionString,
                Container = settings.BlobSettings.ContainerNameExchangeRates
            }, flurlFactory, cache);
        }

        [FunctionName("Currency")]
        public async Task<IActionResult> UpdateCurrencyHttp(
            [HttpTrigger(AuthorizationLevel.Function, "POST", Route = "currency")] HttpRequest req, ILogger log)
        {
            log.LogInformation("HTTP Currency Updated Trigger");
            await _command.Update();

            return new OkObjectResult("OK");
        }
    }
}
