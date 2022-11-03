using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Headstart.Common.Services;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.EasyPost.Mappers;
using OrderCloud.Integrations.EasyPost.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.EasyPost
{
    public interface IEasyPostShippingService : IShippingService
    {
        HSShippingProfiles Profiles { get; }
    }

    public class EasyPostShippingService : IEasyPostShippingService
    {
        private readonly EasyPostSettings easyPostSettings;
        private readonly EasyPostClient easyPostClient;

        public EasyPostShippingService(EasyPostSettings easyPostSettings, EasyPostClient easyPostClient)
        {
            this.easyPostSettings = easyPostSettings;
            Profiles = new HSShippingProfiles(easyPostSettings.FedexAccountId);
            this.easyPostClient = easyPostClient;
        }

        public HSShippingProfiles Profiles { get; }

        public async Task<HSShipEstimateResponse> GetRates(IEnumerable<IGrouping<AddressPair, HSLineItem>> groupedLineItems)
        {
            // First, filter out any line items that are set to have free shipping
            var filteredGroupedList = new List<Grouping<AddressPair, HSLineItem>>();
            foreach (IGrouping<AddressPair, HSLineItem> group in groupedLineItems)
            {
                var filteredLineItems = group.ToList().Where(li => li.Product.xp.FreeShipping == false);
                filteredGroupedList.Add(new Grouping<AddressPair, HSLineItem>(group.Key, filteredLineItems));
            }

            var easyPostShipments = filteredGroupedList.Select(li => EasyPostMappers.MapShipment(li, Profiles)).ToList();
            var easyPostResponses = new List<EasyPostShipment[]>();

            var postShipments = easyPostShipments;
            foreach (var shipment in postShipments)
            {
                var response = await Throttler.RunAsync(shipment, 200, 10, easyPostClient.PostShipment);
                easyPostResponses.Add(response.ToArray());
            }

            var shipEstimateResponse = new HSShipEstimateResponse
            {
                ShipEstimates = groupedLineItems.Select((lineItems, index) =>
                {
                    // If all line items in the list have FreeShipping, then Mock rates
                    if (lineItems.ToList().All(li => li.Product?.xp?.FreeShipping == true))
                    {
                        return MockRatesForFreeShipping(lineItems.ToList());
                    }

                    var firstLi = lineItems.First();
                    var shipMethods = EasyPostMappers.MapRates(easyPostResponses[index]);
                    return new HSShipEstimate()
                    {
                        ID = easyPostResponses[index][0].id,
                        ShipMethods = shipMethods, // This will get filtered down based on carrierAccounts
                        ShipEstimateItems = lineItems.Select(li => new ShipEstimateItem() { LineItemID = li.ID, Quantity = li.Quantity }).ToList(),
                        xp = new ShipEstimateXP
                        {
                            AllShipMethods = shipMethods, // This is being saved so we have all data to compare rates across carrierAccounts
                            SupplierID = firstLi.SupplierID, // This will help with forwarding the supplier order
                            ShipFromAddressID = firstLi.ShipFromAddressID, // This will help with forwarding the supplier order
                        },
                    };
                }).ToList(),
            };
            return shipEstimateResponse;
        }

        public HSShipEstimate MockRatesForFreeShipping(List<HSLineItem> lineItems)
        {
            var firstLi = lineItems.First();
            return new HSShipEstimate
            {
                ID = $"FREE_SHIPPING_{firstLi.SupplierID}",
                ShipMethods = new List<HSShipMethod>
                {
                    new HSShipMethod
                    {
                        ID = $"FREE_SHIPPING_{firstLi.SupplierID}",
                        Cost = 0,
                        Name = "FREE",
                        EstimatedTransitDays = 1, // Can be overwritten by app settings
                    },
                },
                ShipEstimateItems = lineItems.Select(li => new ShipEstimateItem() { LineItemID = li.ID, Quantity = li.Quantity }).ToList(),
                xp = new ShipEstimateXP
                {
                    SupplierID = firstLi.SupplierID, // This will help with forwarding the supplier order
                    ShipFromAddressID = firstLi.ShipFromAddressID, // This will help with forwarding the supplier order
                },
            };
        }
    }
}
