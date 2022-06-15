using System.Collections.Generic;
using Headstart.Common.Models;

namespace OrderCloud.Integrations.EnvironmentSeed.Helpers
{
    public static class Regions
    {
        public static readonly Region UsEast = new Region()
        {
            AzureRegion = "eastus",
            Id = "est",
            Name = "US-East",
        };

        public static readonly Region AustraliaEast = new Region()
        {
            AzureRegion = "australiaeast",
            Id = "aus",
            Name = "Australia-East",
        };

        public static readonly Region EuropeWest = new Region()
        {
            AzureRegion = "westeurope",
            Id = "eur",
            Name = "Europe-West",
        };

        public static readonly Region JapanEast = new Region()
        {
            AzureRegion = "japaneast",
            Id = "jpn",
            Name = "Japan-East",
        };

        public static readonly Region UsWest = new Region()
        {
            AzureRegion = "westus",
            Id = "usw",
            Name = "US-West",
        };

        public static readonly List<Region> AllRegions = new List<Region>()
        {
            UsEast,
            AustraliaEast,
            EuropeWest,
            JapanEast,
            UsWest,
        };
    }
}
