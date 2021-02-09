using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace ordercloud.integrations.freightpop
{

    public class CarrierService
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }
    public class Carrier
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string AccountNickName { get; set; }
        public CarrierService Service { get; set; }
    }

    // used in getRates request
    public class CarrierWithCarrierType
    {
        [Required]
        public int Id { get; set; }
        public string Name { get; set; }
        public string AccountNickName { get; set; }

        // only accepted values are define in the static class
        // CarrierTypeString
        public CarrierTypeString CarrierType { get; set; }
        public List<CarrierService> Services { get; set; }
    }

    public enum CarrierType
    {
        LTL = 1,
        Air = 2,
        Parcel = 3
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum CarrierTypeString
    {
        LTL,
        Air,
        Parcel,
        Truckload,
        OceanFreight
    }

}
