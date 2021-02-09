using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace Headstart.Models
{
    [SwaggerModel]
    public class HSUserGroupAssignment : UserGroupAssignment, IHSObject
    {
        public string ID { get; set; }
    }
}
