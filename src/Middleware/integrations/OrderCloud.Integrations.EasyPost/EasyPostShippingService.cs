using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Flurl.Http;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.EasyPost.Exceptions;
using OrderCloud.Integrations.EasyPost.Mappers;
using OrderCloud.Integrations.EasyPost.Models;
using OrderCloud.Integrations.Library.Interfaces;
using OrderCloud.Integrations.Library.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.EasyPost
{
    public interface IEasyPostShippingService : IShippingService
    {
        HSShippingProfiles Profiles { get; }
    }

    public class EasyPostShippingService : IEasyPostShippingService
    {
        public const string FreeShipping = "FREE_SHIPPING";
        private const string BaseUrl = "https://api.easypost.com/v2";
        private readonly EasyPostConfig config;

        public EasyPostShippingService(EasyPostConfig config, string carrierAccountId)
        {
            this.config = config;
            Profiles = new HSShippingProfiles(carrierAccountId);
        }

        public HSShippingProfiles Profiles { get; }

        public async Task<ShipEstimateResponse> GetRates(IEnumerable<IGrouping<AddressPair, LineItem>> groupedLineItems)
        {
            // First, filter out any line items that are set to have free shipping
            var filteredGroupedList = new List<Grouping<AddressPair, LineItem>>();
            foreach (IGrouping<AddressPair, LineItem> group in groupedLineItems)
            {
                var filteredLineItems = group.ToList().Where(li => li.Product.xp.FreeShipping == false);
                filteredGroupedList.Add(new Grouping<AddressPair, LineItem>(group.Key, filteredLineItems));
            }

            var easyPostShipments = filteredGroupedList.Select(li => EasyPostMappers.MapShipment(li, Profiles)).ToList();
            var easyPostResponses = new List<EasyPostShipment[]>();

            var postShipments = easyPostShipments;
            foreach (var shipment in postShipments)
            {
                var response = await Throttler.RunAsync(shipment, 200, 10, PostShipment);
                easyPostResponses.Add(response.ToArray());
            }

            var shipEstimateResponse = new ShipEstimateResponse
            {
                ShipEstimates = groupedLineItems.Select((lineItems, index) =>
                {
                    // If all line items in the list have FreeShipping, then Mock rates
                    if (lineItems.ToList().All(li => li.Product?.xp?.FreeShipping))
                    {
                        return MockRatesForFreeShipping(lineItems.ToList());
                    }

                    var firstLi = lineItems.First();
                    var shipMethods = EasyPostMappers.MapRates(easyPostResponses[index]);
                    return new ShipEstimate()
                    {
                        ID = easyPostResponses[index][0].id,
                        ShipMethods = shipMethods, // This will get filtered down based on carrierAccounts
                        ShipEstimateItems = lineItems.Select(li => new ShipEstimateItem() { LineItemID = li.ID, Quantity = li.Quantity }).ToList(),
                        xp =
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

        public ShipEstimate MockRatesForFreeShipping(List<LineItem> lineItems)
        {
            var firstLi = lineItems.First();
            return new ShipEstimate
            {
                ID = FreeShipping + $"_{firstLi.SupplierID}",
                ShipMethods = new List<ShipMethod>
                {
                    new ShipMethod
                    {
                        ID = FreeShipping + $"_{firstLi.SupplierID}",
                        Cost = 0,
                        Name = "FREE",
                        EstimatedTransitDays = 1, // Can be overwritten by app settings
                    },
                },
                ShipEstimateItems = lineItems.Select(li => new ShipEstimateItem() { LineItemID = li.ID, Quantity = li.Quantity }).ToList(),
                xp =
                {
                    SupplierID = firstLi.SupplierID, // This will help with forwarding the supplier order
                    ShipFromAddressID = firstLi.ShipFromAddressID, // This will help with forwarding the supplier order
                },
            };
        }

        private async Task<EasyPostShipment> PostShipment(EasyPostShipment shipment)
        {
            try
            {
                return await BaseUrl
                    .WithBasicAuth(config.APIKey, string.Empty)
                    .AppendPathSegment("shipments")
                    .PostJsonAsync(new { shipment })
                    .ReceiveJson<EasyPostShipment>();
            }
            catch (FlurlHttpException ex)
            {
                var error = await ex.GetResponseJsonAsync<EasyPostApiError>();
                throw new EasyPostException(error);
            }
        }
    }
}
