using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ordercloud.integrations.freightpop
{

    public class RateRequestBody
    {
        public List<Accessorial> Accessorials { get; set; }
        [Required]
        public RateAddress ConsigneeAddress { get; set; }
        public List<CarrierWithCarrierType> Carriers { get; set; }

        /* If CarrierType is passed then system will pick some carriers 
        * automatically to get rates. CarrierType should only be passed if
        * list of carriers is not passed. If list of carriers is passed 
        * then we will ignore this. If you want to get the rates of 
        * Truckload and Ocean Freight then you don't need to pass this 
        * and mention it in the PackageType of the first item of Shipment Items detail */
        public CarrierType CarrierType { get; set; }
        [Required]
        public List<Item> Items { get; set; }
        public DateTime ShipDate { get; set; }
        [Required]
        public RateAddress ShipperAddress { get; set; }
    }
}
