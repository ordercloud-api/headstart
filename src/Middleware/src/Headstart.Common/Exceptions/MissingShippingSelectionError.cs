using System.Collections.Generic;

namespace Headstart.Models.Exceptions
{
    public class MissingShippingSelectionError
    {
        public IEnumerable<string> ShipFromAddressIDsRequiringAttention { get; set; }

        public MissingShippingSelectionError(IEnumerable<string> ids)
        {
            ShipFromAddressIDsRequiringAttention = ids;
        }
    }
}