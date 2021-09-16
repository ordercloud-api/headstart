using Cosmonaut.Attributes;
using Headstart.Models;
using ordercloud.integrations.library;
using ordercloud.integrations.library.Cosmos;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;

namespace Headstart.Common.Models
{

    public interface IResourceHistory : ICosmosObject
    {
        ActionType Action { get; set; }
        DateTime DateLastUpdated { get; set; }
        [CosmosPartitionKey]
        string ResourceID { get; set; }
    }

    [SwaggerModel]
    [CosmosCollection("producthistory")]
    public class ProductHistory : CosmosObject, IResourceHistory
    {
        [CosmosPartitionKey]
        public string ResourceID { get; set; }
        public ActionType Action { get; set; }

        public HSProduct Resource { get; set; }
        public DateTime DateLastUpdated { get; set; }
    }

    [SwaggerModel]
    [CosmosCollection("priceschedulehistory")]
    public class PriceScheduleHistory : CosmosObject, IResourceHistory
    {
        [CosmosPartitionKey]
        public string ResourceID { get; set; }
        public ActionType Action { get; set; }

        public PriceSchedule Resource { get; set; }
        public DateTime DateLastUpdated { get; set; }
    }

    public class ProductUpdateData
    {
        //  Product Info
        public string Supplier { get; set; }
        public string ProductID { get; set; }
        public string ProductAction { get; set; }
        public string OldProductType { get; set; }
        public string OldUnitMeasure { get; set; }
        public Nullable<int> OldUnitQty { get; set; }
        public bool? OldActiveStatus { get; set; }
        public string NewProductType { get; set; }
        public string NewUnitMeasure { get; set; }
        public Nullable<int> NewUnitQty { get; set; }
        public bool? NewActiveStatus { get; set; }

        //  Price Schedule info
        public string DefaultPriceScheduleID { get; set; }
        public string DefaultPriceScheduleAction { get; set; }
        public Nullable<int> OldMinQty { get; set; }
        public Nullable<int> OldMaxQty { get; set; }
        public string OldPriceBreak { get; set; }
        public Nullable<int> NewMinQty { get; set; }
        public Nullable<int> NewMaxQty { get; set; }
        public string NewPriceBreak { get; set; }

    }

    public enum ActionType
    {
        Create,
        Update
    }

}
