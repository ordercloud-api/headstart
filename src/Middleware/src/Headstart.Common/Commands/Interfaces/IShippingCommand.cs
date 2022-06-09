using System.Threading.Tasks;
using Headstart.Common.Models;

namespace Headstart.Common.Commands
{
    public interface IShippingCommand
    {
        Task<HSShipEstimateResponse> GetRatesAsync(HSOrderWorksheet worksheet, CheckoutIntegrationConfiguration config = null);
    }
}
