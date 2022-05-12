using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace Headstart.Models
{
    
    public class HSProductAssignment : ProductAssignment, IHSObject
    {
        public string ID { get; set; }
    }
}
