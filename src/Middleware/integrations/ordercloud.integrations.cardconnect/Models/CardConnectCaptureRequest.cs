using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.cardconnect
{
    public class CardConnectCaptureRequest
    {
        // https://developer.cardconnect.com/cardconnect-api#capture-request
        public string merchid { get; set; } // required
        public string retref { get; set; } // required
        public string currency { get; set; } // required
        public string authcode { get; set; }
        public string amount { get; set; }
        public string invoiceid { get; set; }
        public string receipt { get; set; }
    }

    public class CardConnectCaptureResponse
    {
        // https://developer.cardconnect.com/cardconnect-api#capture-response
        public string merchid { get; set; }
        public string account { get; set; }
        public string orderId { get; set; }
        public string amount { get; set; }
        public string retref { get; set; }
        public string batchid { get; set; }
        public string setlstat { get; set; }
        public dynamic receipt { get; set; }
        public string respstat { get; set; }
        public string resptext { get; set; }
        public string respcode { get; set; }
    }
}
