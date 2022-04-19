using OrderCloud.SDK;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using Headstart.Common.Models.Base;
using Headstart.Models.Attributes;
using ordercloud.integrations.easypost;
using System.ComponentModel.DataAnnotations;
using ordercloud.integrations.exchangerates;
using ordercloud.integrations.library.intefaces;
using Headstart.Common.Models.Headstart.Extended;

namespace Headstart.Common.Models.Headstart
{
	public class SuperHsProduct : HsBaseObject
	{
		public HsProduct Product { get; set; } = new HsProduct();

		public PriceSchedule PriceSchedule { get; set; } = new PriceSchedule();
		public IList<Spec> Specs { get; set; } = new List<Spec>();

		public IList<HsVariant> Variants { get; set; } = new List<HsVariant>();
	}

	public class SuperHsMeProduct : HsBaseObject
	{
		public HsMeProduct Product { get; set; } = new HsMeProduct();
		public PriceSchedule PriceSchedule { get; set; } = new PriceSchedule();
		public IList<Spec> Specs { get; set; } = new List<Spec>();
		public IList<HsVariant> Variants { get; set; } = new List<HsVariant>();
	}

	public class PartialHsProduct : PartialProduct<ProductXp>
	{
	}

	public class HsLineItemProduct : LineItemProduct<ProductXp>
	{
	}

	public class HsProduct : Product<ProductXp>
	{
		public string Id { get; set; } = string.Empty;
	}

	public class HsMeProduct : BuyerProduct<ProductXp, HsPriceSchedule>
	{
	}

	public class HsVariant : Variant<HsVariantXp>
	{
	}

	public class ProductXp
	{
		#region DO NOT DELETE
		[OrchestrationIgnore]
		public dynamic IntegrationData { get; set; }

		public Dictionary<string, List<string>> Facets = new Dictionary<string, List<string>>();
		#endregion

		[System.ComponentModel.DataAnnotations.Required]
		public ObjectStatus? Status { get; set; }

		public bool HasVariants { get; set; }

		[MaxLength(500), OrchestrationIgnore]
		public string Note { get; set; } = string.Empty;

		public TaxCategorization Tax { get; set; } = new TaxCategorization();

		public UnitOfMeasure UnitOfMeasure { get; set; } = new UnitOfMeasure();

		public ProductType ProductType { get; set; } = new ProductType();

		public SizeTier SizeTier { get; set; } = new SizeTier();

		public bool IsResale { get; set; }

		public List<ProductAccessorial> Accessorials { get; set; } = new List<ProductAccessorial>();

		[JsonConverter(typeof(StringEnumConverter))]
		public CurrencySymbol? Currency { get; set; }

		public bool? ArtworkRequired { get; set; }

		public bool PromotionEligible { get; set; }

		public bool FreeShipping { get; set; }

		public string FreeShippingMessage { get; set; } = string.Empty;

		public List<ImageAsset> Images { get; set; } = new List<ImageAsset>();

		public List<DocumentAsset> Documents { get; set; } = new List<DocumentAsset>();
	}

	public class ImageAsset
	{
		public string Url { get; set; } = string.Empty;

		public string ThumbnailUrl { get; set; } = string.Empty;

		public List<string> Tags { get; set; } = new List<string>();
	};

	public class DocumentAsset
	{
		public string Url { get; set; } = string.Empty;

		public string FileName { get; set; } = string.Empty;
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum ProductType
	{
		Standard,
		Quote
	}

	public class HsVariantXp
	{
		public string SpecCombo { get; set; } = string.Empty;

		public List<HsSpecValue> SpecValues { get; set; } = new List<HsSpecValue>();

		public string NewId { get; set; } = string.Empty;

		public List<ImageAsset> Images { get; set; } = new List<ImageAsset>();
	}


	public class HsSpecValue
	{
		public string SpecName { get; set; } = string.Empty;

		public string SpecOptionValue { get; set; } = string.Empty;

		public string PriceMarkup { get; set; } = string.Empty;
	}

	public enum ProductAccessorial
	{
		Freezable = 6,
		Hazmat = 7,
		KeepFromFreezing = 8,
	}
}