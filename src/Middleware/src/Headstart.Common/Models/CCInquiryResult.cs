namespace Headstart.Common.Models
{
    public class CCInquiryResult
    {
        public bool CanVoid { get; set; }

        public bool CanRefund { get; set; }

        public bool PendingCapture { get; set; }

        public string CaptureDate { get; set; }
    }
}
