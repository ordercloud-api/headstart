using Newtonsoft.Json;
using ordercloud.integrations.library;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;
using System;
using Headstart.Models.Headstart;

namespace Headstart.Common.Models
{
    public class RMA : CosmosObject
    {
        public string PartitionKey { get; set; }
        public string SourceOrderID { get; set; }
        public decimal TotalCredited { get; set; }
        public decimal ShippingCredited { get; set; }
        public string RMANumber { get; set; }
        public string SupplierID { get; set; }
        public string SupplierName { get; set; }
        public RMAType Type { get; set; }
        public DateTime DateCreated { get; set; }
        public DateTime? DateComplete { get; set; }
        public RMAStatus Status { get; set; }
        public List<RMALineItem> LineItems { get; set; }
        public List<RMALog> Logs { get; set; }
        public string FromBuyerID { get; set; }
        public string FromBuyerUserID { get; set; }
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
        public string ID { get; set; }
        public int QuantityRequested { get; set; }
        public int QuantityProcessed { get; set; }
        public RMALineItemStatus Status { get; set; }
        public string Reason { get; set; }
        public string Comment { get; set; }
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
        public string FromUserID { get; set; }
    }

    public class RMARefundRequestBody
    {
        public List<RMALineItemRefundRequestBody> LineItemsToRefund { get; set; }
    }

    public class RMALineItemRefundRequestBody
    {
        public string LineItemID { get; set; }
        public int? PercentToRefund { get; set; }
    }

    public class RMAWithLineItemStatusByQuantity
    {
        public string SupplierOrderID { get; set; }
        public RMA RMA { get; set; }
        public List<LineItemStatusChanges> LineItemStatusChangesList { get; set; }
    }
}