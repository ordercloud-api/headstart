using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using OrderCloud.Integrations.Library.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Library.Interfaces
{
    public interface IShippingService
    {
        Task<ShipEstimateResponse> GetRates(IEnumerable<IGrouping<AddressPair, LineItem>> groupedLineItems);
    }
}
