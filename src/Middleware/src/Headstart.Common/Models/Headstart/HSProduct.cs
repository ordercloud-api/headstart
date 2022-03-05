using OrderCloud.SDK;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using Headstart.Models.Extended;
using Headstart.Models.Attributes;
using ordercloud.integrations.easypost;
using ordercloud.integrations.exchangerates;
using System.ComponentModel.DataAnnotations;
using ordercloud.integrations.library.intefaces;

namespace Headstart.Models
{
    public class SuperHSProduct : IHSObject
    {
        public string ID { get; set; } = string.Empty;

        public HSProduct Product { get; set; } = new HSProduct();

        public PriceSchedule PriceSchedule { get; set; } = new PriceSchedule();
        public IList<Spec> Specs { get; set; } = new List<Spec>();

        public IList<HSVariant> Variants { get; set; } = new List<HSVariant>();
    }

    public class SuperHSMeProduct : IHSObject
    {
        public string ID { get; set; }
        public HSMeProduct Product { get; set; }
        public PriceSchedule PriceSchedule { get; set; }
        public IList<Spec> Specs { get; set; }
        public IList<HSVariant> Variants { get; set; }
    }

    public class PartialHSProduct : PartialProduct<ProductXp> { }

    public class HSLineItemProduct : LineItemProduct<ProductXp> { }

    public class HSProduct : Product<ProductXp>, IHSObject { }

    public class HSMeProduct : BuyerProduct<ProductXp, HSPriceSchedule> { }

    public class HSVariant : Variant<HSVariantXp> { }

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

        public TaxCategorization Tax { get; set; }

        public UnitOfMeasure UnitOfMeasure { get; set; } = new UnitOfMeasure();

        public ProductType ProductType { get; set; }

        public SizeTier SizeTier { get; set; }

        public bool IsResale { get; set; }

        public List<ProductAccessorial> Accessorials { get; set; }

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

    public class HSVariantXp
    {
        public string SpecCombo { get; set; } = string.Empty;

        public List<HSSpecValue> SpecValues { get; set; } = new List<HSSpecValue>();

        public string NewID { get; set; } = string.Empty;

        public List<ImageAsset> Images { get; set; } = new List<ImageAsset>();
    }


    public class HSSpecValue
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