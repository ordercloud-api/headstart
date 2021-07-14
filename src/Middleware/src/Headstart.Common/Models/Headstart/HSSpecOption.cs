using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace Headstart.Models
{
    
    public class HSSpecOption : SpecOption<SpecOptionXp>, IHSObject
    {
    }

    
    public class SpecOptionXp
    {
        public string Description { get; set; }
        public string SpecID { get; set; }
    }
}
