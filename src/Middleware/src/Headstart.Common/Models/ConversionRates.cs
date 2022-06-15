using System.Collections.Generic;

namespace Headstart.Common.Models
{
    public class ConversionRates
    {
        public CurrencyCode BaseCode { get; set; }

        public List<ConversionRate> Rates { get; set; }
    }
}
