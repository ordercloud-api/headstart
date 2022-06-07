namespace Headstart.Common.Models
{
    public class CCTransactionResult
    {
        /// <summary>
        /// Did the transaction succeed?.
        /// </summary>
        public bool Succeeded { get; set; }

        /// <summary>
        /// The amount of the transaction.
        /// </summary>
        public decimal Amount { get; set; }

        /// <summary>
        /// The processor-generated ID for this action. Null if a create attempt failed.
        /// </summary>
        public string TransactionID { get; set; }

        /// <summary>
        /// The raw processor-specific response code. Depending on the processor, typical meaninings include Approved, Declined, Held For Review, Retry, Error.
        /// </summary>
        public string ResponseCode { get; set; }

        /// <summary>
        /// The authorization code granted by the card issuing bank for this transaction. Should be 6 characters, e.g. "HH5414".
        /// </summary>
        public string AuthorizationCode { get; set; }

        /// <summary>
        /// A code explaining the result of address verification (AVS). Whether to perform AVS is typically configured at the processor level. Standard 1 character result codes, see https://www.merchantmaverick.com/what-is-avs-for-credit-card-processing/.
        /// </summary>
        public string AVSResponseCode { get; set; }

        /// <summary>
        /// User readable text explaining the result.
        /// </summary>
        public string Message { get; set; }

        public string merchid { get; set; }
    }
}
