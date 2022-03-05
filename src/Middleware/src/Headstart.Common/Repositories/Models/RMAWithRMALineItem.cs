using Headstart.Common.Models;

namespace Headstart.Common.Repositories.Models
{
    public class RMAWithRMALineItem
    {
        public RMA RMA { get; set; } = new RMA();

        public RMALineItem RMALineItem { get; set; } = new RMALineItem();
    }
}