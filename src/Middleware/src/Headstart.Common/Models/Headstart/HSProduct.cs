using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Headstart.Models.Extended;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrderCloud.Integrations.EasyPost.Mappers;
using OrderCloud.Integrations.ExchangeRates.Models;
using OrderCloud.Integrations.Library.Attributes;
using OrderCloud.Integrations.Library.Interfaces;
using OrderCloud.SDK;

namespace Headstart.Models
{
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ProductType
    {
        Standard,
        Quote,
    }

    public enum ProductAccessorial
    {
        Freezable = 6,
        Hazmat = 7,
        KeepFromFreezing = 8,
    }

    public class SuperHSProduct : IHSObject
    {
        public string ID { get; set; }

        public HSProduct Product { get; set; }

        public PriceSchedule PriceSchedule { get; set; }

        public IList<Spec> Specs { get; set; }

        public IList<HSVariant> Variants { get; set; }
    }

    public class SuperHSMeProduct : IHSObject
    {
        public string ID { get; set; }

        public HSMeProduct Product { get; set; }

        public PriceSchedule PriceSchedule { get; set; }

        public IList<Spec> Specs { get; set; }

        public IList<HSVariant> Variants { get; set; }
    }

    public class PartialHSProduct : PartialProduct<ProductXp>
    {
    }

    public class HSLineItemProduct : LineItemProduct<ProductXp>
    {
    }

    public class HSProduct : Product<ProductXp>, IHSObject
    {
    }

    public class HSMeProduct : BuyerProduct<ProductXp, HSPriceSchedule>
    {
    }

    public class HSVariant : Variant<HSVariantXp>
    {
    }

    public class ProductXp
    {
        // DO NOT DELETE
        [OrchestrationIgnore]
        public dynamic IntegrationData { get; set; }

        // DO NOT DELETE
        public Dictionary<string, List<string>> Facets { get; set; } = new Dictionary<string, List<string>>();

        [System.ComponentModel.DataAnnotations.Required]
        public ObjectStatus? Status { get; set; }

        public bool HasVariants { get; set; }

        [MaxLength(500), OrchestrationIgnore]
        public string Note { get; set; }

        public TaxCategorization Tax { get; set; }

        public UnitOfMeasure UnitOfMeasure { get; set; } = new UnitOfMeasure();

        public ProductType ProductType { get; set; }

        public SizeTier SizeTier { get; set; }

        public bool IsResale { get; set; } = false;

        public List<ProductAccessorial> Accessorials { get; set; }

        [JsonConverter(typeof(StringEnumConverter))]
        public CurrencySymbol? Currency { get; set; } = null;

        public bool? ArtworkRequired { get; set; } = false;

        public bool PromotionEligible { get; set; }

        public bool FreeShipping { get; set; }

        public string FreeShippingMessage { get; set; }

        public List<ImageAsset> Images { get; set; }

        public List<DocumentAsset> Documents { get; set; }
    }

    public class ImageAsset
    {
        public string Url { get; set; }

        public string ThumbnailUrl { get; set; }

        public List<string> Tags { get; set; }
    }

    public class DocumentAsset
    {
        public string Url { get; set; }

        public string FileName { get; set; }
    }

    public class HSVariantXp
    {
        public string SpecCombo { get; set; }

        public List<HSSpecValue> SpecValues { get; set; }

        public string NewID { get; set; }

        public List<ImageAsset> Images { get; set; }
    }

    public class HSSpecValue
    {
        public string SpecName { get; set; }

        public string SpecOptionValue { get; set; }

        public string PriceMarkup { get; set; }
    }
}
