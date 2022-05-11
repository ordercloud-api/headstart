using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace Headstart.Models
{
    public class HSAddressAssignment : AddressAssignment, IHSObject
    {
        public string ID { get; set; }
    }
}
