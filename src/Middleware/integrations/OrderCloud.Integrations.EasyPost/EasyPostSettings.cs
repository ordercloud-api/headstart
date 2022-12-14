namespace OrderCloud.Integrations.EasyPost
{
    public class EasyPostSettings
    {
        public string ApiKey { get; set; }

        /// <summary>
        /// Name of person that is expected to sign for packages at customs (include both first and last name).
        /// Required if EnvironmentSettings:ShippingProvider is set to "EasyPost".
        /// </summary>
        public string CustomsSigner { get; set; }

        /// <summary>
        /// The FedEx carrier account identifier
        /// Either this or UpsAccountId must be defined if EnvironmentSettings:ShippingProvider is set to "EasyPost".
        /// </summary>
        public string FedexAccountId { get; set; }

        /// <summary>
        /// If the order has free shipping, this marks the transit days for the order
        /// Optional - defaults to 3 days.
        /// </summary>
        public int FreeShippingTransitDays { get; set; } = 3;

        /// <summary>
        /// The fallback cost for shipping if no rates are returned
        /// Optional - defaults to 20.
        /// </summary>
        public decimal NoRatesFallbackCost { get; set; } = 20;

        /// <summary>
        /// The number of transit days to set on a shipping estimate if no rates are returned
        /// Optional - defaults to 3.
        /// </summary>
        public int NoRatesFallbackTransitDays { get; set; } = 3;

        /// <summary>
        /// The USPS carrier account identifier
        /// Either this or FedexAccountId must be defined if EnvironmentSettings:ShippingProvider is set to "EasyPost".
        /// </summary>
        public string USPSAccountId { get; set; }
    }
}
