using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.API.Controllers
{
    /// <summary>
    /// Payment commands in Headstart.
    /// </summary>
    [Route("payments")]
    public class PaymentController : CatalystController
    {
        private readonly IPaymentCommand paymentCommand;

        public PaymentController(IPaymentCommand paymentCommand)
        {
            this.paymentCommand = paymentCommand;
        }

        /// <summary>
        /// Save payments. Creates or updates payments as needed for this order.
        /// </summary>
        [HttpPut, Route("{orderID}/update"), OrderCloudUserAuth(ApiRole.Shopper)]
        public async Task<IList<HSPayment>> SavePayments(string orderID, [FromBody] PaymentUpdateRequest request)
        {
            return await paymentCommand.SavePayments(orderID, request.Payments, UserContext.AccessToken);
        }
    }
}
