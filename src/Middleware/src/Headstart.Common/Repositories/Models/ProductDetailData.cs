using System;
using OrderCloud.SDK;
using ordercloud.integrations.library;
using Headstart.Common.Models.Headstart;

namespace Headstart.Common.Repositories.Models
{
	public class ProductDetailData : CosmosObject
	{
		public string PartitionKey { get; set; } = string.Empty;

		public string ProductId { get; set; } = string.Empty;

		public Product Product { get; set; } = new Product();

		public HSProductDetail Data { get; set; } = new HSProductDetail();

		public ProductSaleDetail ProductSales { get; set; } = new ProductSaleDetail();
	}

	public class HSProductDetail
	{
		public string SupplierId { get; set; } = string.Empty;

		public string SupplierName { get; set; } = string.Empty;

		public string Active { get; set; } = string.Empty;

		public string Status { get; set; } = string.Empty;

		public string Note { get; set; } = string.Empty;

		public string TaxCategory { get; set; } = string.Empty;

		public string TaxCode { get; set; } = string.Empty;

		public string TaxDescription { get; set; } = string.Empty;

		public long UnitOfMeasureQty { get; set; }

		public string UnitOfMeasure { get; set; } = string.Empty;

		public string ProductType { get; set; } = string.Empty;

		public string SizeTier { get; set; } = string.Empty;

		public bool Resale { get; set; }

		public string Currency { get; set; } = string.Empty;

		public bool? ArtworkRequired { get; set; }

		public string VariantId { get; set; } = string.Empty;

		public bool VariantActive { get; set; }

		public string SpecName { get; set; } = string.Empty;

		public string VariantName { get; set; } = string.Empty;

		public string SpecCombo { get; set; } = string.Empty;

		public string SpecOptionValue { get; set; } = string.Empty;

		public string SpecPriceMarkup { get; set; } = string.Empty;

		public bool VariantLevelTracking { get; set; }

		public int? InventoryQuantity { get; set; }

		public DateTimeOffset? InventoryLastUpdated { get; set; }

		public bool InventoryOrderCanExceed { get; set; }

		public string ProductDateCreated { get; set; } = string.Empty;

		public string PriceScheduleId { get; set; } = string.Empty;

		public string PriceScheduleName { get; set; } = string.Empty;

		public HsPriceSchedule PriceScheduleOverride { get; set; } = new HsPriceSchedule();

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