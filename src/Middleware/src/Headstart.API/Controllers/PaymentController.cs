using Headstart.API.Commands;
using Headstart.Common.Models;
using Headstart.Models.Attributes;
using Headstart.Models.Headstart;
using Microsoft.AspNetCore.Mvc;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Headstart.Common.Controllers
{
    /// <summary>
    /// Payment commands in Headstart
    /// </summary>
    [Route("payments")]
    public class PaymentController : CatalystController
    {

        private readonly IPaymentCommand _command;
        private readonly AppSettings _settings;
        public PaymentController(IPaymentCommand command, AppSettings settings)
        {
            _command = command;
            _settings = settings;
        }

        /// <summary>
        /// Save payments. Creates or updates payments as needed for this order.
        /// </summary>
        [HttpPut, Route("{orderID}/update"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<IList<HSPayment>> SavePayments(string orderID, [FromBody] PaymentUpdateRequest request)
        {
            return await _command.SavePayments(orderID, request.Payments, UserContext.AccessToken);
        }
    }
}
