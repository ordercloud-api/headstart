using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Models;
using OrderCloud.SDK;

namespace Headstart.Common.Services
{
    public class DefaultShippingService : IShippingService
    {
        public Task<ShipEstimateResponse> GetRates(IEnumerable<IGrouping<AddressPair, LineItem>> groupedLineItems)
        {
            throw new NotImplementedException();
        }
    }
}
