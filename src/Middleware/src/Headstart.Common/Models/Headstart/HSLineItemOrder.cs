using ordercloud.integrations.library;

namespace Headstart.Models.Headstart
{
    [SwaggerModel]
    public class HSLineItemOrder
    {
        public HSOrder HSOrder { get; set; }
        public HSLineItem HSLineItem { get; set; }
    }
}
