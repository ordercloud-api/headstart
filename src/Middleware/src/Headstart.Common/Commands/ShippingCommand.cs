using System.Threading.Tasks;
using Headstart.Common.Models;

namespace Headstart.Common.Commands
{
    public class ShippingCommand : IShippingCommand
    {
        public ShippingCommand()
        {
        }

        public async Task<HSShipEstimateResponse> GetRatesAsync(HSOrderWorksheet worksheet, CheckoutIntegrationConfiguration config = null)
        {
            return await Task.FromResult(new HSShipEstimateResponse());
        }
    }
}
