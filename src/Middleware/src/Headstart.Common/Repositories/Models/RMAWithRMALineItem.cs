using Headstart.Common.Models;
using ordercloud.integrations.library;

namespace Headstart.Common.Repositories.Models
{
    public class RMAWithRMALineItem
    {
        public RMA RMA { get; set; }
        public RMALineItem RMALineItem { get; set; }
    }
}
