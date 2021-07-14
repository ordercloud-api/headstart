namespace ordercloud.integrations.cardconnect
{
    public class CardConnectVoidRequest
    {
        public string currency { get; set; }
        public string merchid { get; set; }
        public string retref { get; set; } // The retrieval reference number from the original authorization response.
        public string amount { get; set; } //  if equal to $0, the full amount is voided. Defaults to 0 to support full amount voiding
    }

    public class CardConnectVoidResponse
    {
        public string merchid { get; set; }
        public decimal? amount { get; set; }
        public string orderId { get; set; }
        public string retref { get; set; }
        public string authcode { get; set; }
        public string respcode { get; set; }
        public string respproc { get; set; }
        public string respstat { get; set; }
        public string resptext { get; set; }
    }

}
