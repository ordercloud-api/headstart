using System;
using Headstart.Common.Models;
using OrderCloud.Integrations.EnvironmentSeed.Models;

namespace OrderCloud.Integrations.EnvironmentSeed.Helpers
{
    public static class MarketplaceHelper
    {
        public static string GetApiUrl(OrderCloudEnvironment environment, string regionName)
        {
            var region = GetRegion(regionName);

            if (environment == OrderCloudEnvironment.Production)
            {
                if (region == Regions.UsEast)
                {
                    return @"https://useast-production.ordercloud.io";
                }
                else if (region == Regions.AustraliaEast)
                {
                    return @"https://australiaeast-production.ordercloud.io";
                }
                else if (region == Regions.EuropeWest)
                {
                    return @"https://westeurope-production.ordercloud.io";
                }
                else if (region == Regions.JapanEast)
                {
                    return @"https://japaneast-production.ordercloud.io";
                }
                else if (region == Regions.UsWest)
                {
                    return @"https://api.ordercloud.io";
                }
                else
                {
                    throw new NotImplementedException("Cannot create marketplace config with invalid region.");
                }
            }
            else if (environment == OrderCloudEnvironment.Sandbox)
            {
                if (region == Regions.UsEast)
                {
                    return @"https://useast-sandbox.ordercloud.io";
                }
                else if (region == Regions.AustraliaEast)
                {
                    return @"https://australiaeast-sandbox.ordercloud.io";
                }
                else if (region == Regions.EuropeWest)
                {
                    return @"https://westeurope-sandbox.ordercloud.io";
                }
                else if (region == Regions.JapanEast)
                {
                    return @"https://japaneast-sandbox.ordercloud.io";
                }
                else if (region == Regions.UsWest)
                {
                    return @"https://sandboxapi.ordercloud.io";
                }
                else
                {
                    throw new NotImplementedException("Cannot create marketplace config with invalid region.");
                }
            }
            else
            {
                throw new NotImplementedException("Cannot create marketplace config with invalid environment.");
            }
        }

        public static Region GetRegion(string regionName)
        {
            return Regions.AllRegions.Find(r => r.Name.Equals(regionName.Trim(), StringComparison.OrdinalIgnoreCase));
        }
    }
}
