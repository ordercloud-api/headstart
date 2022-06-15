namespace OrderCloud.Integrations.CardConnect.Models
{
    /// <summary>
    /// https://developer.cardpointe.com/cardconnect-api#capture
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1629:Documentation text should end with a period", Justification = "URL reference")]
    public class CardConnectCaptureRequest
    {
        /// <summary>
        /// Merchant ID, required for all requests. Must match merchid of transaction to be captured.
        /// </summary>
        public string merchid { get; set; } // required

        /// <summary>
        /// CardPointe retrieval reference number from authorization response.
        /// </summary>
        public string retref { get; set; } // required

        public string currency { get; set; } // required

        public string authcode { get; set; }

        public string amount { get; set; }

        public string invoiceid { get; set; }

        public string receipt { get; set; }
    }

    /// <summary>
    /// https://developer.cardpointe.com/cardconnect-api#capture-response
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1629:Documentation text should end with a period", Justification = "URL reference")]
    public class CardConnectCaptureResponse : CardConnectResponseData
    {
        /// <summary>
        /// Copied from the capture request.
        /// </summary>
        public string merchid { get; set; }

        /// <summary>
        /// Masked account number.
        /// </summary>
        public string account { get; set; }

        public string orderId { get; set; }

        /// <summary>
        /// The amount included in the capture request.
        /// </summary>
        public string amount { get; set; }

        /// <summary>
        /// The retref included in the capture request.
        /// </summary>
        public string retref { get; set; }

        public string batchid { get; set; }

        /// <summary>
        /// The current settlement status. The settlement status changes throughout the transaction lifecycle, from authorization to settlement. The following values can be returned in the capture response:
        /// Note: See Settlement Status Response Values for a complete list of setlstat values.
        /// - Authorized: The authorization was approved, but the transaction has not yet been captured.
        /// - Declined: The authorization was declined; therefore, the transaction can not be captured.
        /// - Queued for Capture: The authorization was approved and captured but has not yet been sent for settlement.
        /// - Voided: The authorization was voided; therefore, the transaction cannot be captured.
        /// - Zero Amount: The authorization was a $0 auth for account validation, which cannot be captured.
        /// </summary>
        public string setlstat { get; set; }

        public dynamic receipt { get; set; }
    }
}
