using Headstart.API.Commands;
using Headstart.Common.Models;
using Headstart.Models.Headstart;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Headstart.Common.Controllers
{
    /// <summary>
    /// Payment commands in Headstart.
    /// </summary>
    [Route("payments")]
    public class PaymentController : CatalystController
    {
        private readonly IPaymentCommand command;
        private readonly AppSettings settings;

        public PaymentController(IPaymentCommand command, AppSettings settings)
        {
            this.command = command;
            this.settings = settings;
        }

        /// <summary>
        /// Save payments. Creates or updates payments as needed for this order.
        /// </summary>
        [HttpPut, Route("{orderID}/update"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<IList<HSPayment>> SavePayments(string orderID, [FromBody] PaymentUpdateRequest request)
        {
            return await command.SavePayments(orderID, request.Payments, UserContext.AccessToken);
        }
    }
}
