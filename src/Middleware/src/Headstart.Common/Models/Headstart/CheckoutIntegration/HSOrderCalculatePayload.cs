namespace Headstart.Common.Models
{
    public class HSOrderCalculatePayload
    {
        public HSOrderWorksheet OrderWorksheet { get; set; }

        public CheckoutIntegrationConfiguration ConfigData { get; set; }
    }
}
