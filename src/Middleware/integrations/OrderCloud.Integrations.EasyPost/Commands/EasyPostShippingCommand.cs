using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Commands;
using Headstart.Common.Extensions;
using Headstart.Common.Models;
using OrderCloud.Integrations.EasyPost.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.EasyPost.Commands
{
    public class EasyPostShippingCommand : IShippingCommand
    {
        private readonly IEasyPostShippingService shippingService;
        private readonly IOrderCloudClient orderCloudClient;
        private readonly EasyPostSettings easyPostSettings;

        public EasyPostShippingCommand(IEasyPostShippingService shippingService, EasyPostSettings easyPostSettings, IOrderCloudClient orderCloudClient)
        {
            this.shippingService = shippingService;
            this.orderCloudClient = orderCloudClient;
            this.easyPostSettings = easyPostSettings;
        }

        public async Task<HSShipEstimateResponse> GetRatesAsync(HSOrderWorksheet worksheet, dynamic config = null)
        {
            var groupedLineItems = worksheet.LineItems.GroupBy(li => new AddressPair { ShipFrom = li.ShipFromAddress, ShipTo = li.ShippingAddress }).ToList();

            // include all accounts at this stage so we can save on order worksheet and analyze
            var shipResponse = (await shippingService.GetRates(groupedLineItems)).Reserialize<HSShipEstimateResponse>();

            // Certain suppliers use certain shipping accounts. This filters available rates based on those accounts.
            for (var i = 0; i < groupedLineItems.Count; i++)
            {
                var supplierID = groupedLineItems[i].First().SupplierID;
                var profile = shippingService.Profiles.FirstOrDefault(supplierID);
                var methods = FilterMethodsBySupplierConfig(shipResponse.ShipEstimates[i].ShipMethods.Where(s => profile.CarrierAccountIDs.Contains(s.xp.CarrierAccountID)).ToList(), profile);
                shipResponse.ShipEstimates[i].ShipMethods = methods.Select(s =>
                {
                    // there is logic here to support not marking up shipping over list rate. But USPS is always list rate
                    // so adding an override to the suppliers that use USPS
                    var carrier = shippingService.Profiles.ShippingProfiles.First(p => p.CarrierAccountIDs.Contains(s.xp?.CarrierAccountID));
                    s.Cost = carrier.MarkupOverride ?
                        s.xp.OriginalCost * carrier.Markup :
                        Math.Min(s.xp.OriginalCost * carrier.Markup, s.xp.ListRate);

                    return s;
                }).ToList();
            }

            await shipResponse.ShipEstimates
                .CheckForEmptyRates(easyPostSettings.NoRatesFallbackCost, easyPostSettings.NoRatesFallbackTransitDays)
                .ApplyShippingLogic(worksheet, orderCloudClient, easyPostSettings.FreeShippingTransitDays);

            return shipResponse;
        }

        private IEnumerable<HSShipMethod> FilterMethodsBySupplierConfig(List<HSShipMethod> methods, EasyPostShippingProfile profile)
        {
            // will attempt to filter out by supplier method specs, but if there are filters and the result is none and there are valid methods still return the methods
            if (profile.AllowedServiceFilter.Count == 0)
            {
                return methods;
            }

            var filtered_methods = methods.Where(s => profile.AllowedServiceFilter.Contains(s.xp.ServiceName)).Select(s => s).ToList();
            return filtered_methods.Any() ? filtered_methods : methods;
        }
    }
}
