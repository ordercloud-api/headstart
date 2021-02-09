using Cosmonaut.Attributes;
using Newtonsoft.Json;
using ordercloud.integrations.library;
using System.Collections.Generic;
using Newtonsoft.Json.Converters;

namespace Headstart.Common.Models
{
    [SwaggerModel]
    [CosmosCollection("rmas")]
    public class RMA : CosmosObject
    {
        [CosmosPartitionKey]
        public string PartitionKey { get; set; }
        public string RMANumber { get; set; }
        public string SupplierID { get; set; }
        public RMAType Type { get; set; }
        public string DateCreated { get; set; }
        public RMAStatus Status { get; set; }
        public List<RMALineItem> LineItems { get; set; }
        public List<RMALog> Logs { get; set; }
        public List<RMACredit> CreditsApplied { get; set; }
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
        Complete,
        Canceled
    }

    [SwaggerModel]
    public class RMALineItem
    {
        public string ID { get; set; }
        public RMALineItemStatus Status { get; set; }
        public string Reason { get; set; }
        public string Comment { get; set; }
        public bool IsResolved { get; set; }
    }


    [JsonConverter(typeof(StringEnumConverter))]
    public enum RMALineItemStatus
    {
        Requested,
        Processing,
        RequestCanceled,
        Approved,
        Denied,
        PartialQtyApproved
    }

    [SwaggerModel]
    public class RMALog
    {
        public RMAStatus Status { get; set; }
        public string Date { get; set; }
        public string Comment { get; set; }
        public string FromUserID { get; set; }
    }

    [SwaggerModel]
    public class RMACredit
    {
        public string PaymentID { get; set; }
        public string TransactionID { get; set; }
        public string TransactionDate { get; set; }
        public decimal SupplierCredit { get; set; }
        public decimal TotalRefunded { get; set; }
    }
}