using System.Collections.Generic;

namespace Headstart.Common.Models
{
    public class ShipmentTestRequest
    {
        public HSOrder Order { get; set; }

        public List<HSLineItem> LineItems { get; set; }
    }
}
