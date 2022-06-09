namespace OrderCloud.Integrations.EasyPost
{
    public class EasyPostSettings
    {
        public string APIKey { get; set; }

        public string FedexAccountId { get; set; }

        public int FreeShippingTransitDays { get; set; }

        public decimal NoRatesFallbackCost { get; set; }

        public int NoRatesFallbackTransitDays { get; set; }

        public string USPSAccountId { get; set; }
    }
}
