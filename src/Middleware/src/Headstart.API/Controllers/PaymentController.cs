using Headstart.API.Commands;
using Headstart.API.Controllers;
using Headstart.Common.Models;
using Headstart.Models.Attributes;
using Headstart.Models.Headstart;
using Microsoft.AspNetCore.Mvc;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Headstart.Common.Controllers
{
    [DocComments("\"Headstart Orders\" for handling payment commands in Headstart")]
    [HSSection.Headstart(ListOrder = 2)]
    [Route("payments")]
    public class PaymentController : BaseController
    {

        private readonly IPaymentCommand _command;
        private readonly AppSettings _settings;
        public PaymentController(IPaymentCommand command, AppSettings settings) : base(settings)
        {
            _command = command;
            _settings = settings;
        }

        [DocName("Save payments")]
        [DocComments("Creates or updates payments as needed for this order")]
        [HttpPut, Route("{orderID}/update"), OrderCloudIntegrationsAuth(ApiRole.Shopper)]
        public async Task<IList<HSPayment>> SavePayments(string orderID, [FromBody] PaymentUpdateRequest request)
        {
            return await _command.SavePayments(orderID, request.Payments, VerifiedUserContext.AccessToken);
        }
    }
}
