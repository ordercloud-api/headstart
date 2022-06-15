namespace OrderCloud.Integrations.CardConnect.Models
{
    public class CardConnectResponseData
    {
        /// <summary>
        /// Alpha-numeric response code that represents the description of the response.
        /// </summary>
        public string respcode { get; set; }

        /// <summary>
        /// Abbreviation that represents the platform and the processor for the transaction.
        /// </summary>
        public string respproc { get; set; }

        /// <summary>
        /// Indicates the status of the request. Can be one of the following values:
        /// - A: Approved
        /// - B: Retry
        /// - C: Declined.
        /// </summary>
        public string respstat { get; set; }

        /// <summary>
        /// Text description of response.
        /// </summary>
        public string resptext { get; set; }
    }
}
