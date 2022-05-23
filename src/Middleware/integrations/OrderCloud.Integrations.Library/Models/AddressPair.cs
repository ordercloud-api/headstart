using System;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Library.Models
{
    public class AddressPair : IEquatable<AddressPair>
    {
        public Address ShipFrom { get; set; }

        public Address ShipTo { get; set; }

        public bool Equals(AddressPair other)
        {
            // we still want to compare the rest of these properties to handle one time addresses
            return (ShipFrom.ID == other.ShipFrom.ID) &&
                    (ShipFrom.Street1 == other?.ShipFrom.Street1) &&
                    (ShipFrom.Zip == other?.ShipFrom.Zip) &&
                    (ShipFrom.City == other?.ShipFrom.City) &&
                    (ShipTo.Street1 == other?.ShipTo.Street1) &&
                    (ShipTo.Zip == other?.ShipTo.Zip) &&
                    (ShipTo.City == other?.ShipTo.City);
        }

        public override int GetHashCode()
        {
            return 1; // force Equals to be called for comparison
        }
    }
}
