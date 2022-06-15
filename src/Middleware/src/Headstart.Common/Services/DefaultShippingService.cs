using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Models;

namespace Headstart.Common.Services
{
    public class DefaultShippingService : IShippingService
    {
        public Task<HSShipEstimateResponse> GetRates(IEnumerable<IGrouping<AddressPair, HSLineItem>> groupedLineItems)
        {
            throw new NotImplementedException();
        }
    }
}
