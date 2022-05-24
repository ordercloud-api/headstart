using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Smarty
{
    public class CheckoutIntegrationController : CatalystController
    {
        public CheckoutIntegrationController()
        {
        }

        [Route("shippingrates")]
        [HttpPost]
        [OrderCloudWebhookAuth]
        public async Task<ShipEstimateResponse> GetShippingRates([FromBody] SmartyStreetsConfig orderCalculatePayload)
        {
            return await Task.FromResult(new ShipEstimateResponse());
        }
    }
}
