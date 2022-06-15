namespace Headstart.Jobs.Settings
{
    public class JobSettings
    {
        public string CaptureCreditCardsAfterDate { get; set; } // TODO: remove this once all orders have IsPaymentCaptured set

        public bool ShouldCaptureCreditCardPayments { get; set; }

        public bool ShouldRunZoho { get; set; }
    }
}
