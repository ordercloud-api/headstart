using System;

namespace ordercloud.integrations.freightpop
{
    public class ShippingRate
    {
        public string Id { get; set; }
        public string AccountName { get; set; }
        public string Carrier { get; set; }
        public string Currency { get; set; }
        public DateTimeOffset? DeliveryDate { get; set; }
        public int DeliveryDays { get; set; }
        public string QuoteId { get; set; }
        public string CarrierQuoteId { get; set; }
        public string Service { get; set; }
        
        // what is list cost?
        public decimal ListCost { get; set; }
        public decimal TotalCost { get; set; }
    }

}
