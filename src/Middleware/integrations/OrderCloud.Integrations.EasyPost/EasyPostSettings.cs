namespace OrderCloud.Integrations.EasyPost
{
    public class EasyPostSettings
    {
        public string ApiKey { get; set; }

        /// <summary>
        /// Name of person that is expected to sign for packages at customs (include both first and last name).
        /// </summary>
        public string CustomsSigner { get; set; }

        public string FedexAccountId { get; set; }

        public int FreeShippingTransitDays { get; set; }

        public decimal NoRatesFallbackCost { get; set; }

        public int NoRatesFallbackTransitDays { get; set; }

        public string USPSAccountId { get; set; }

    }
}
