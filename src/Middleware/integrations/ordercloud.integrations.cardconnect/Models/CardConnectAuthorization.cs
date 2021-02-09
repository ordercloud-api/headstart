using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using OrderCloud.SDK;

namespace ordercloud.integrations.cardconnect
{
    public class CreditCardAuthorization
    {
        [ApiReadOnly]
        public ResponseStatus Status { get; set; } // respstat
        public string ReferenceNumber { get; set; } // retref
        public string Account { get; set; } // account
        [OrderCloud.SDK.Required]
        public string ExpirationDate { get; set; } // expiry
        public string Token { get; set; } // token
        [OrderCloud.SDK.Required]
        public decimal? Amount { get; set; } // amount
        [OrderCloud.SDK.Required]
        public string MerchantID { get; set; } // merchid
        [ApiReadOnly]
        public string ResponseCode { get; set; } // respcode
        [ApiReadOnly]
        public string ResponseText { get; set; } // resptext
        [ApiReadOnly]
        public string ResponseProcessor { get; set; } // respproc
        [ApiReadOnly]
        public string AVSResponseCode { get; set; } // avsresp
        [ApiReadOnly]
        public CVVResponse CVVResponseCode { get; set; } // cvvresp
        [ApiReadOnly]
        public BinType BinType { get; set; }
        [ApiReadOnly]
        public string AuthorizationCode { get; set; } // authcode
        //public string Signature { get; set; } // signature
        [ApiReadOnly]
        public bool CommercialCard { get; set; } // commcard
        [ApiReadOnly]
        public dynamic Receipt { get; set; } // receipt

        // request properties
        public string OrderID { get; set; } // orderid
        public string Currency { get; set; } = "USD"; // currency default to "USD" is not provided
        [OrderCloud.SDK.Required]
        public string CardHolderName { get; set; } // name
        [EmailAddress]
        public string CardHolderEmail { get; set; }
        [OrderCloud.SDK.Required]
        public string Address { get; set; } // address
        [OrderCloud.SDK.Required]
        public string City { get; set; } // city
        [OrderCloud.SDK.Required]
        public string Region { get; set; } // region
        [MaxLength(2, ErrorMessage = "Invalid Country Code")]
        [MinLength(2, ErrorMessage = "Invalid Country Code")]
        [OrderCloud.SDK.Required]
        public string Country { get; set; } // country
        [OrderCloud.SDK.Required]
        public string PostalCode { get; set; } // postal
        [OrderCloud.SDK.Required]
        [MaxLength(4, ErrorMessage = "Invalid CVV Code")]
        [MinLength(3, ErrorMessage = "Invalid CVV Code")]
        public string CVV { get; set; } // cvv2
        //[JsonIgnore] public string ecomind { get; set; } = "E";
        //[JsonIgnore] public string capture { get; set; } = "N";
        //[JsonIgnore] public string bin { get; set; } = "N";

    }



    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum ResponseStatus
    {
        Approved,
        Retry,
        Declined
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum CVVResponse
    {
        Valid,
        Invalid,
        NotProcessed,
        NotPresent,
        NotCertified
    }

    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum BinType
    {
        Corporate,
        FSAPrepaid,
        GSAPurchase,
        Prepaid,
        PrepaidCorporate,
        PrepaidPurchase,
        Purchase,
        Invalid
    }
}
