using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Models;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
{
    public class MockShippingCommand : IShippingCommand
    {
        public MockShippingCommand()
        {
        }

        public async Task<HSShipEstimateResponse> GetRatesAsync(HSOrderWorksheet worksheet, CheckoutIntegrationConfiguration config = null)
        {
            try
            {
                var groupedLineItems = worksheet.LineItems.GroupBy(li => new AddressPair { ShipFrom = li.ShipFromAddress, ShipTo = li.ShippingAddress }).ToList();

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
                        return new HSShipEstimate()
                        {
                            ID = $"ShipEstimate{index}",
                            ShipMethods = GetMockShipMethods(),
                            ShipEstimateItems = lineItems.Select(li => new ShipEstimateItem() { LineItemID = li.ID, Quantity = li.Quantity }).ToList(),
                            xp = new ShipEstimateXP
                            {
                                SupplierID = firstLi.SupplierID, // This will help with forwarding the supplier order
                                ShipFromAddressID = firstLi.ShipFromAddressID, // This will help with forwarding the supplier order
                            },
                        };
                    }).ToList(),
                };
                return await Task.FromResult(shipEstimateResponse);
            }
            catch (Exception ex)
            {
                throw ex;
            }
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
                xp =
                {
                    SupplierID = firstLi.SupplierID, // This will help with forwarding the supplier order
                    ShipFromAddressID = firstLi.ShipFromAddressID, // This will help with forwarding the supplier order
                },
            };
        }

        public List<HSShipMethod> GetMockShipMethods()
        {
            return new List<HSShipMethod>
            {
                new HSShipMethod
                {
                    ID = "StandardShipping",
                    Name = "Standard Shipping",
                    Cost = 10m,
                    EstimatedTransitDays = 5,
                    xp = new ShipMethodXP
                    {
                        FreeShippingApplied = false,
                    },
                },
                new HSShipMethod
                {
                    ID = "ExpressShipping",
                    Name = "Express Shipping",
                    Cost = 20m,
                    EstimatedTransitDays = 2,
                    xp = new ShipMethodXP
                    {
                        FreeShippingApplied = false,
                    },
                },
            };
        }
    }
}
