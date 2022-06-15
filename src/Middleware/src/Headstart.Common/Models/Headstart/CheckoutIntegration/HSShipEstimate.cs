using System.Collections.Generic;
using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class HSShipEstimate : ShipEstimate<ShipEstimateXP, HSShipMethod>
    {
    }

    public class ShipEstimateXP
    {
        public IList<HSShipMethod> AllShipMethods { get; set; }

        public string SupplierID { get; set; }

        public string ShipFromAddressID { get; set; }
    }
}
