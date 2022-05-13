using OrderCloud.SDK;
using System.Collections.Generic;

namespace OrderCloud.Integrations.Smarty.Models
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

        // https://smartystreets.com/docs/cloud/us-street-api#footnotes
        public string GapBetweenRawAndValid { get; set; }
    }
}
