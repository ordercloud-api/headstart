using System.Collections.Generic;
using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class BuyerAddressValidation
    {
        public BuyerAddressValidation(BuyerAddress raw)
        {
            RawAddress = raw;
        }

        public BuyerAddress RawAddress { get; set; }

        public BuyerAddress ValidAddress { get; set; }

        public bool ValidAddressFound => ValidAddress != null;

        public List<BuyerAddress> SuggestedAddresses { get; set; } = new List<BuyerAddress>() { };

        public string GapBetweenRawAndValid { get; set; }
    }
}
