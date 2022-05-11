using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace Headstart.Models
{
    
    public class HSUserGroupAssignment : UserGroupAssignment, IHSObject
    {
        public string ID { get; set; }
    }
}
