using ordercloud.integrations.library;

namespace Headstart.Models.Headstart
{
    
    public class HSLineItemOrder
    {
        public HSOrder HSOrder { get; set; }
        public HSLineItem HSLineItem { get; set; }
    }
}
