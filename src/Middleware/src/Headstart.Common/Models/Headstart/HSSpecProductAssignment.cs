using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace Headstart.Models
{
    [SwaggerModel]
    public class HSSpecProductAssignment : SpecProductAssignment, IHSObject
    {
        public string ID { get; set; }
    }
}
