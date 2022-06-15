namespace OrderCloud.Integrations.CardConnect.Models
{
    /// <summary>
    /// https://developer.cardpointe.com/cardconnect-api#authorization-request
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1629:Documentation text should end with a period", Justification = "URL reference")]
    public class CardConnectAuthorizationRequest
    {
        /// <summary>
        /// Required. CardPointe merchant ID, required for all requests.
        /// </summary>
        public string merchid { get; set; }

        /// <summary>
        /// Optional. Source system order number.
        /// Note: If you include an order ID it must meet the following requirements:
        /// - The order ID must be a unique value. Using duplicate order IDs can lead to the wrong transaction being voided in the event of a timeout.
        /// - The order ID must not include any portion of a payment account number (PAN), and no portion of the order ID should be mistaken for a PAN. If the order ID passes the Luhn check performed by the CardPointe Gateway, the value will be masked in the database, and attempts to use the order ID in an inquire, void, or refund request will fail.
        /// </summary>
        public string orderid { get; set; }

        /// <summary>
        /// Required.  Can be:
        /// - CardSecure Token - A token representing a payment account number. See the CardSecure Developer Guide for more information.
        /// - Clear text card number
        /// Note: Only PCI Level 1 and Level 2 compliant merchants should handle clear text card numbers in authorization requests.It is strongly recommended that you tokenize the clear text card data before passing it in an authorization request.
        /// - Bank Account Number - Account(s) must be entitled with electronic check capability. When using this field, the bankaba field is also required.
        /// Note: To use a stored profile, omit the account property and supply the profile ID in the profile field instead. See (https://developer.cardpointe.com/cardconnect-api#profiles) for more information.
        /// </summary>
        public string account { get; set; }

        /// <summary>
        /// Required. Optional for eCheck(ACH) or digital wallet(for example, Apple Pay or Google Pay) payments.
        /// Card expiration in one of the following formats
        /// - MMYY
        /// - YYYYM (for single-digit months)
        /// - YYYYMM
        /// - YYYYMMDD
        /// </summary>
        public string expiry { get; set; }

        /// <summary>
        /// Required. Amount with decimal or without decimal in currency minor units (for example, USD Pennies or EUR Cents).
        /// The value can be a positive or negative amount or 0, and is used to identify the type of authorization, as follows:
        /// - Positive - Authorization request.
        /// -Zero - Account Verification request, if AVS and CVV verification is enabled for your merchant account.Account Verification is not supported for eCheck(ACH) authorizations.
        /// - Negative - Refund without reference(Forced Credit). Merchants must be configured to process forced credit transactions.To refund an existing authorization, use Refund (https://developer.cardpointe.com/cardconnect-api#refund).
        /// </summary>
        public string amount { get; set; }

        /// <summary>
        /// Optional, Defaults to USD. Currency of the authorization (for example, USD for US dollars or CAD for Canadian Dollars).
        /// Note: If specified in the auth request, the currency value must match the currency that the MID is configured for. Specifying the incorrect currency will result in a "Wrong currency for merch" response.
        /// </summary>
        public string currency { get; set; }

        /// <summary>
        /// Optional. Account holder's name, optional for credit cards and electronic checks (ACH).
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// Optional (Required for AVS Verification). Account holder's street address.
        /// </summary>
        public string address { get; set; }

        /// <summary>
        /// Optional. Account holder's city.
        /// </summary>
        public string city { get; set; }

        /// <summary>
        /// Optional. Account holder's region, US State, Mexican State, Canadian Province.
        /// </summary>
        public string region { get; set; }

        /// <summary>
        /// Optional. (Required for all non-US addresses) Account holder's country (2-character country code), defaults to "US".
        /// </summary>
        public string country { get; set; }

        /// <summary>
        /// Optional. (Required for AVS Verification) The account holder's postal code.
        /// If country is "US", must be 5 or 9 digits.Otherwise any alphanumeric string is accepted.Defaults to "55555" if not included in the request or stored profile.
        /// Note: It is strongly recommended that your application prompts for postal code for card-not-present transactions to prevent fraud and downgrades, and to avoid declines on the CardPointe Gateway if the AVS verification option is enabled for your merchant account.
        /// </summary>
        public string postal { get; set; }

        /// <summary>
        /// Optional.  Defaults to E. A transaction origin indicator, for card-not-present and eCheck (ACH) transactions only.
        /// For ProfitStars ACH transactions, see Making an ACH Authorization Request for more information.
        /// For card-not-present transactions, one of the following values:
        /// - T: telephone or mail payment
        /// - R: recurring billing
        /// - E: e-commerce web or mobile application
        /// </summary>
        public string ecomind { get; set; } = "E";

        /// <summary>
        /// Optional. The 3 or 4-digit cardholder verification value (CVV2/CVC/CID) value present on the card.
        /// Note: It is strongly recommended that your application prompts for CVV for card-not-present transactions for fraud prevention and to avoid declines on the CardPointe Gateway if the CVV verification option is enabled for your merchant account.
        /// </summary>
        public string cvv2 { get; set; }

        /// <summary>
        /// Optional, to create an account profile or to use an existing profile.
        /// To create a profile using the account holder data provided in the request, specify Y.
        /// To use an existing profile for this authorization, omit the account parameter and instead use the profile parameter to supply the 20-digit profile id and 1-3-digit account id string in the format. &lt;profileid&gt;/\&lt;acctid&gt;. See Profiles for more information.
        /// Note: You can submit a $0 authorization, including CVV and AVS verification, to validate the customer's information before creating a profile.
        /// </summary>
        public string profile { get; set; }

        public string capture { get; set; }
    }

    /// <summary>
    /// https://developer.cardpointe.com/cardconnect-api#authorization-response
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1629:Documentation text should end with a period", Justification = "URL reference")]
    public class CardConnectAuthorizationResponse : CardConnectResponseData
    {
        /// <summary>
        /// A token that replaces the card number in capture and settlement requests, if requested.
        /// </summary>
        public string token { get; set; }

        public string account { get; set; }

        /// <summary>
        /// The unique retrieval reference number, used to identify and manage the transaction.
        /// </summary>
        public string retref { get; set; }

        /// <summary>
        /// Authorized amount. Same as the request amount for most approvals.
        /// The amount remaining on the card for prepaid/gift cards if partial authorization is enabled.
        /// Not relevant for declines.
        /// </summary>
        public decimal? amount { get; set; }

        /// <summary>
        /// The payment card expiration date, in MMYY format, if expiry was included in the request.
        /// </summary>
        public string expiry { get; set; }

        /// <summary>
        /// Copied from the authorization request.
        /// </summary>
        public string merchid { get; set; }

        /// <summary>
        /// Alpha-numeric AVS (zip code) verification response code.
        /// Note: avsresp is typically only returned for approved authorizations, however this field can be returned in the response for a declined authorization if this setting is enabled for the merchant account.
        /// </summary>
        public string avsresp { get; set; }

        /// <summary>
        ///     Alpha-numeric CVV (card verification value) verification response code, if returned by the processor.
        /// One of the following values:
        /// - M: Valid CVV Match.
        /// - N: Invalid CVV.
        /// - P: CVV Not Processed.
        /// - S: Merchant indicated that the CVV is not present on the card.
        /// - U: Card issuer is not certified and/or has not provided Visa encryption keys.
        /// - X or blank: No response.
        /// Note: cvvresp is typically only returned for approved authorizations, however this field can be returned in the response for a declined authorization if this setting is enabled for the merchant account.
        /// </summary>
        public string cvvresp { get; set; }

        /// <summary>
        /// SON escaped, Base64 encoded, Gzipped, BMP of signature data. Returned if the authorization used a token that had associated signature data or track data with embedded signature data.
        /// </summary>
        public string signature { get; set; }

        /// <summary>
        /// Possible Values:
        /// - Corp
        /// - FSA+Prepaid
        /// - GSA+Purchase
        /// - Prepaid
        /// - Prepaid+Corp
        /// - Prepaid+Purchase
        /// - Purchase
        /// </summary>
        public string bintype { get; set; }

        /// <summary>
        /// Y if a Corporate or Purchase Card.
        /// </summary>
        public string commcard { get; set; }

        /// <summary>
        /// Authorization Response Cryptogram (ARPC). This is returned only when EMV data is present within the Track Parameter.
        /// </summary>
        public string emv { get; set; }

        /// <summary>
        /// An array that includes the fields returned in the BIN response.
        /// </summary>
        public BinInfo binInfo { get; set; }

        /// <summary>
        /// Returned when the request includes "receipt":y".
        /// A stringifed array that includes additional fields to be printed on a receipt.
        /// See Receipt Data Fields, below, for a list of the possible fields returned.
        /// </summary>
        public dynamic receipt { get; set; }

        /// <summary>
        /// Authorization Code from the Issuer.
        /// </summary>
        public string authcode { get; set; }
    }

    public class BinInfo
    {
        public string country { get; set; }

        public string product { get; set; }

        public string bin { get; set; }

        public string cardusestring { get; set; }

        public bool gsa { get; set; }

        public bool corporate { get; set; }

        public bool fsa { get; set; }

        public string subtype { get; set; }

        public bool purchase { get; set; }

        public bool prepaid { get; set; }

        public string issuer { get; set; }

        public string binlo { get; set; }

        public string binhi { get; set; }
    }
}
