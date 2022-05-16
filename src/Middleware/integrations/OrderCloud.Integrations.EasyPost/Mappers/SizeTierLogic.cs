using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using OrderCloud.Integrations.EasyPost.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.EasyPost.Mappers
{
    // Note, this is not an EasyPost-specific concept.
    // It was originally created by Bill Hickey as a way to get less expensive shipping rates from FreightPop by grouping lineItems.
    // Its not currently being used anywhere because the concept doesn't fit with the EasyPost API particularly well.
    // But, I'm saving it in case its useful at some point.

    // measured in how many of the product fit in a 22x22x22 box
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum SizeTier
    {
        // ships alone
        G,

        // 2-5
        A,

        // 5-15
        B,

        // 15-49
        C,

        // 50-99
        D,

        // 100-999
        E,

        // 1000+
        F,
    }

    public static class SmartPackageMapper
    {
        // Any parcel sent to easy post with a dimension over 33 returns no rates.
        // Dimensions around 33 return high rates in the $200's for the slowest shipping tiers
        private static readonly int EasyPostMaxParcelDimension = 33; // inches.

        // 22 inches seems to produce rates around $20 for the slowest tiers, which feels reasonable
        private static readonly int TargetReasonableParcelDimension = 22; // inches

        private static readonly Dictionary<SizeTier, double> SizeFactorMap = new Dictionary<SizeTier, double>()
        {
            { SizeTier.A, .385 }, // 38.5% of a full package
            { SizeTier.B, .10 },
            { SizeTier.C, .031 },
            { SizeTier.D, .0134 },
            { SizeTier.E, .0018 },
            { SizeTier.F, .00067 },
        };

        public static List<EasyPostParcel> MapLineItemsIntoPackages(List<LineItem> lineItems)
        {
            var lineItemsThatCanShipTogether = lineItems.Where(li => li.Product.xp.SizeTier != SizeTier.G).OrderBy(lineItem => lineItem.Product.xp.SizeTier);
            var lineItemsThatShipAlone = lineItems.Where(li => li.Product.xp.SizeTier == SizeTier.G);

            var parcels = lineItemsThatCanShipTogether
                .SelectMany(lineItem => Enumerable.Repeat(lineItem, lineItem.Quantity))
                .Aggregate(new List<Package>(), (packages, item) =>
                {
                    if (packages.Count == 0)
                    {
                        packages.Add(new Package());
                    }

                    var percentFillToAdd = (double)SizeFactorMap[item.Product.xp.SizeTier];
                    var currentPackage = packages.Last();
                    if (currentPackage.PercentFilled + percentFillToAdd > 1)
                    {
                        var newPackage = new Package() { PercentFilled = percentFillToAdd, Weight = (decimal)item.ShipWeightOrDefault(0) };
                        packages.Add(newPackage);
                    }
                    else
                    {
                        currentPackage.PercentFilled += percentFillToAdd;
                        currentPackage.Weight += (decimal)item.ShipWeightOrDefault(0);
                    }

                    return packages;
                });

            var combinationPackages = parcels.Select((package, index) =>
            {
                var dimension = (double)Math.Ceiling(package.PercentFilled * Package.FullPackageDimension);
                return new EasyPostParcel()
                {
                    weight = (double)package.Weight,
                    length = Math.Max(dimension, Package.FullPackageDimension),
                    width = Math.Max(dimension, Package.FullPackageDimension),
                    height = Math.Max(dimension, Package.FullPackageDimension),
                };
            }).Select(p => CapParcelDimensions(p, TargetReasonableParcelDimension));

            var individualPackages = lineItemsThatShipAlone
                .SelectMany(lineItem => Enumerable.Repeat(lineItem, lineItem.Quantity))
                .Select(li => new EasyPostParcel()
                {
                    // length/width/height cannot be zero otherwise we'll get an error (422 Unprocessable Entity) from easypost
                    weight = li.ShipWeightOrDefault(Package.DefaultWeight), // (double) (li.Product.ShipWeight ?? Package.DEFAULT_WEIGHT),
                    length = li.ShipLengthOrDefault(Package.FullPackageDimension), // (double) (li.Product.ShipLength.IsNullOrZero() ? Package.FULL_PACKAGE_DIMENSION : li.Product.ShipLength),
                    width = li.ShipWidthOrDefault(Package.FullPackageDimension), // (double) (li.Product.ShipWidth.IsNullOrZero() ? Package.FULL_PACKAGE_DIMENSION : li.Product.ShipWidth),
                    height = li.ShipHeightOrDefault(Package.FullPackageDimension), // (double) (li.Product.ShipHeight.IsNullOrZero() ? Package.FULL_PACKAGE_DIMENSION : li.Product.ShipHeight),
                }).Select(p => CapParcelDimensions(p, EasyPostMaxParcelDimension));

            return combinationPackages.Union(individualPackages).ToList();
        }

        private static EasyPostParcel CapParcelDimensions(EasyPostParcel parcel, double maximumDimension)
        {
            parcel.height = Math.Min(parcel.height, maximumDimension);
            parcel.width = Math.Min(parcel.width, maximumDimension);
            parcel.length = Math.Min(parcel.length, maximumDimension);
            return parcel;
        }
    }

    public class Package
    {
        // This is intended to be a max parcel dimension, but is not being honored because of a bug in the math. see line 83 about percentage 1 vs 100
        // however, fixing that math produces some crazy rates (~$1000).
        public static readonly double FullPackageDimension = 11; // inches.
        public static readonly double DefaultWeight = 5;

        public double PercentFilled { get; set; } = 0;

        public decimal Weight { get; set; } = 0; // lbs
    }
}
