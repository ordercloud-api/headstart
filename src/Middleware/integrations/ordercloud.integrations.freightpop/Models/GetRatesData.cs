using System.Collections.Generic;

namespace ordercloud.integrations.freightpop
{
    public class GetRatesData
    {
        public List<ShippingRate> Rates { get; set; }
        public List<string> ErrorMessages { get; set; }
    }
}
