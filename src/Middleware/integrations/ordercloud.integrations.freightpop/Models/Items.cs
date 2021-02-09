using System.ComponentModel.DataAnnotations;

namespace ordercloud.integrations.freightpop
{
    public class Item
    {
        // freightPOP docs indicate that description is only required for some carriers
        [Required]
        public string Description { get; set; }
        // freightPOP docs indicate that this is required for local shipment
        [Required]
        public decimal FreightClass { get; set; }
        [Required]
        public decimal Height { get; set; }
        [Required]
        public decimal Length { get; set; }
        [Required]
        public PackageType PackageType { get; set; }
        [Required]
        public int Quantity { get; set; }
        [Required]
        public decimal Weight { get; set; }
        [Required]
        public Unit Unit { get; set; }
        [Required]
        public decimal Width { get; set; }
        public string PackageId { get; set; }
    }

    // same as item, but includes innerpieces
    public class ShipmentItem
    {
        public InnerPieces InnerPieces { get; set; }
        public string Description { get; set; }
        public decimal FreightClass { get; set; }
        public decimal Height { get; set; }
        public decimal Length { get; set; }
        public PackageType PackageType { get; set; }
        public int Quantity { get; set; }
        public decimal Weight { get; set; }
        public Unit Unit { get; set; }
        public decimal Width { get; set; }
        public string PackageId { get; set; }
    }
    public class InnerPieces
    {
        public PackageType PackageType { get; set; }
        public int Quantity { get; set; }
    }
    public enum PackageType
    {
        Barrel = 1,
        Box = 2,
        Bundle = 3,
        Container = 4,
        Crate = 5,
        DhlExpressEnvelope = 6,
        FedexEnvelope = 7,
        FedexParcel = 8,
        Flatbed = 9,
        Intermodal = 10,
        Pail = 11,
        Pallet = 12,
        Truckload = 13,
        UpsEnvelope= 14,
        UpsParcel = 15,
        Truckload_26ft = 16,
        Ocean20ftCon = 17,
        Ocean40fthcCon = 18,
        Ocean40ftCon = 19,
        Ocean45fthcCon = 20
    }

    public enum Accessorial
    {
        DeliveryAppointment = 1,
        DestinationExhibition = 2,
        DestinationInsideDelivery = 3,
        DestinationLiftGate = 4,
        DestinationSortAndSegregatee = 5,
        Freezable = 6,
        Hazmat = 7,
        KeepFromFreezing = 8,
        LimitedAccessDelivery = 9,
        LimitedAccessPickup = 10,
        OriginExhibition = 11,
        OriginInsidePickup = 12,
        OriginLiftGate = 13,
        OriginSortAndSegregatee = 14,
        ResidentialDelivery = 15,
        ResidentialPickup = 16
    }
    public enum Unit
    {
        kg_cm = 1,
        lbs_inch = 2
    }
}
