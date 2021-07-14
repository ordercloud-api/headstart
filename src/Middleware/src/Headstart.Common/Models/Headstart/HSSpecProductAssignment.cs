using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace Headstart.Models
{
    
    public class HSSpecProductAssignment : SpecProductAssignment, IHSObject
    {
        public string ID { get; set; }
    }
}
