using System.Collections.Generic;
using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class AddressValidation
    {
        public AddressValidation(Address raw)
        {
            RawAddress = raw;
        }

        public Address RawAddress { get; set; }

        public Address ValidAddress { get; set; }

        public bool ValidAddressFound => ValidAddress != null;

        public List<Address> SuggestedAddresses { get; set; } = new List<Address>() { };

        public string GapBetweenRawAndValid { get; set; }
    }
}
