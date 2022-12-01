using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;

namespace Headstart.API.Controllers
{
    public class OrderReturnIntegrationEventController : CatalystController
    {
        private readonly IOrderReturnIntegrationEventCommand orderReturnIntegrationEventCommand;

        public OrderReturnIntegrationEventController(IOrderReturnIntegrationEventCommand orderReturnIntegrationEventCommand)
        {
            this.orderReturnIntegrationEventCommand = orderReturnIntegrationEventCommand;
        }

        [Route("calculateorderreturn")]
        [HttpPost]
        [OrderCloudWebhookAuth]
        public async Task<HSCalculateOrderReturnResponse> CalculateOrderReturn([FromBody] HSCalculateOrderReturnPayload payload)
        {
            return await orderReturnIntegrationEventCommand.CalculateOrderReturn(payload.OrderWorksheet, payload.OrderReturn);
        }
    }
}
