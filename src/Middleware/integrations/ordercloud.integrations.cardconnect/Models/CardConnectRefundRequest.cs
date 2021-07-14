using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.cardconnect
{
    // https://developer.cardconnect.com/cardconnect-api#refund-request
    public class CardConnectRefundRequest
    {
        public string merchid { get; set; } // required
        public string retref { get; set; } // required
        public string currency { get; set; } // required
        public string amount { get; set; } // Amount to refund.  If not set, full amount of the transaction is refunded.
        public string orderid { get; set; }
        public string receipt { get; set; }
    }

    // https://developer.cardconnect.com/cardconnect-api#refund-response
    public class CardConnectRefundResponse
    {
        public string merchid { get; set; }
        public decimal? amount { get; set; }
        public string retref { get; set; }
        public string orderId { get; set; }
        public string respcode { get; set; }
        public string respproc { get; set; }
        public string respstat { get; set; }
        public string resptext { get; set; }
        public string receipt { get; set; }
    }
}
