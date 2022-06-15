using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    public class HSShipMethod : ShipMethod<ShipMethodXP>
    {
        public HSShipMethod()
        {
            xp = new ShipMethodXP();
        }
    }

    public class ShipMethodXP
    {
        public string Carrier { get; set; } // e.g. "Fedex"

        public string CarrierAccountID { get; set; }

        public decimal ListRate { get; set; }

        public bool Guaranteed { get; set; }

        public decimal OriginalCost { get; set; }

        public bool FreeShippingApplied { get; set; }

        public int? FreeShippingThreshold { get; set; }

        public CurrencyCode? OriginalCurrency { get; set; }

        public CurrencyCode? OrderCurrency { get; set; }

        public double? ExchangeRate { get; set; }
    }
}
