using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.cardconnect
{
    // https://developer.cardconnect.com/cardconnect-api#inquire-by-order-id-request
    public class CardConnectInquireRequest
    {
        public string merchid { get; set; } // required
        public string orderid { get; set; } // required
        public string currency { get; set; } // required
        public string retref { get; set; } // required
        public string set { get; set; }
    }

    // https://developer.cardconnect.com/cardconnect-api#inquire-by-order-id-response
    public class CardConnectInquireResponse
    {
        public string amount { get; set; }
        public string orderId { get; set; }
        public string resptext { get; set; }
        public string setlstat { get; set; }
        public string respcode { get; set; }
        public string merchid { get; set; }
        public string token { get; set; }
        public string resproc { get; set; }
        public string authdate { get; set; }
        public string lastfour { get; set; }
        public string name { get; set; }
        public string expiry { get; set; }
        public string currency { get; set; }
        public string retref { get; set; }
        public string respstat { get; set; }
        public string account { get; set; }
        public string bintype { get; set; }
        public string entrymode { get; set; }
        public string emvTagData { get; set; }
        public dynamic receipt { get; set; }
        public string userfield { get; set; }
        public string voidable { get; set; }
        public string refundable { get; set; }
        public string capturedate { get; set; }
        public string batchid { get; set; }
        public string authcode { get; set; }
    }
}
