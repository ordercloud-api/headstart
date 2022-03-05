using System;
using Newtonsoft.Json;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using Headstart.Models.Headstart;
using ordercloud.integrations.library;

namespace Headstart.Common.Models
{
    public class RMA : CosmosObject
    {
        public string PartitionKey { get; set; } = string.Empty;

        public string SourceOrderID { get; set; } = string.Empty;

        public decimal TotalCredited { get; set; }

        public decimal ShippingCredited { get; set; }

        public string RMANumber { get; set; } = string.Empty;

        public string SupplierID { get; set; } = string.Empty;

        public string SupplierName { get; set; } = string.Empty;

        public RMAType Type { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime? DateComplete { get; set; }

        public RMAStatus Status { get; set; }

        public List<RMALineItem> LineItems { get; set; } = new List<RMALineItem>();

        public List<RMALog> Logs { get; set; } = new List<RMALog>();

        public string FromBuyerID { get; set; } = string.Empty;

        public string FromBuyerUserID { get; set; } = string.Empty;
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RMAType
    {
        Cancellation,
        Return
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RMAStatus
    {
        Requested,
        Denied,
        Processing,
        Approved,
        Complete,
    }
    
    public class RMALineItem
    {
        public string ID { get; set; } = string.Empty;

        public int QuantityRequested { get; set; }

        public int QuantityProcessed { get; set; }

        public RMALineItemStatus Status { get; set; }

        public string Reason { get; set; } = string.Empty;

        public string Comment { get; set; } = string.Empty;

        public int? PercentToRefund { get; set; }

        public bool RefundableViaCreditCard { get; set; }

        public bool IsResolved { get; set; }

        public bool IsRefunded { get; set; }

        public decimal LineTotalRefund { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum RMALineItemStatus
    {
        Requested,
        Processing,
        Approved,
        Complete,
        Denied,
        PartialQtyApproved,
        PartialQtyComplete
    }
    
    public class RMALog
    {
        public RMAStatus Status { get; set; }

        public DateTime Date { get; set; }

        public decimal? AmountRefunded { get; set; }

        public string FromUserID { get; set; } = string.Empty;
    }

    public class RMARefundRequestBody
    {
        public List<RMALineItemRefundRequestBody> LineItemsToRefund { get; set; } = new List<RMALineItemRefundRequestBody>();
    }

    public class RMALineItemRefundRequestBody
    {
        public string LineItemID { get; set; } = string.Empty;

        public int? PercentToRefund { get; set; }
    }

    public class RMAWithLineItemStatusByQuantity
    {
        public string SupplierOrderID { get; set; } = string.Empty;

        public RMA RMA { get; set; } = new RMA();

        public List<LineItemStatusChanges> LineItemStatusChangesList { get; set; } = new List<LineItemStatusChanges>();
    }
}