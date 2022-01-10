using Headstart.Models;
using ordercloud.integrations.easypost;
using ordercloud.integrations.exchangerates;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;

namespace Headstart.Common.Repositories.Models
{
    public class ProductDetailData : CosmosObject
    {
        public string PartitionKey { get; set; }
        public string ProductID { get; set; }
        public Product Product { get; set; }
        public HSProductDetail Data { get; set; }
        public ProductSaleDetail ProductSales { get; set; }
    }

    public class HSProductDetail
    {
        public string SupplierID { get; set; }
        public string SupplierName { get; set; }
        public string Active { get; set; }
        public string Status { get; set; }
        public string Note { get; set; }
        public string TaxCategory { get; set; }
        public string TaxCode { get; set; }
        public string TaxDescription { get; set; }
        public long UnitOfMeasureQty { get; set; }
        public string UnitOfMeasure { get; set; }
        public string ProductType { get; set; }
        public string SizeTier { get; set; }
        public bool Resale { get; set; }
        public string? Currency { get; set; }
        public bool? ArtworkRequired { get; set; }
        public string VariantID { get; set; }
        public bool VariantActive { get; set; }
        public string SpecName { get; set; }
        public string VariantName { get; set; }
        public string SpecCombo { get; set; }
        public string SpecOptionValue { get; set; }
        public string SpecPriceMarkup { get; set; }
        public bool VariantLevelTracking { get; set; }
        public int? InventoryQuantity { get; set; }
        public DateTimeOffset? InventoryLastUpdated { get; set; }
        public bool InventoryOrderCanExceed { get; set; }
        public string ProductDateCreated { get; set; }
        public string PriceScheduleID { get; set; }
        public string PriceScheduleName { get; set; }
        public HSPriceSchedule PriceScheduleOverride { get; set; }
        public decimal Price { get; set; }
        public decimal Cost { get; set; }
    }
    public class ProductSaleDetail
    {
        public double ThreeMonthQuantity { get; set; }
        public decimal ThreeMonthTotal { get; set; }
        public double SixMonthQuantity { get; set; }
        public decimal SixMonthTotal { get; set; }
        public double TwelveMonthQuantity { get; set; }
        public decimal TwelveMonthTotal { get; set; }
    }
}
