using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Common.Services.Zoho;
using Headstart.Common.Services.Zoho.Mappers;
using Headstart.Common.Services.Zoho.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Library;
using OrderCloud.SDK;
using TaxCategorization = OrderCloud.Integrations.Library.Interfaces.TaxCategorization;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.API.Commands.Zoho
{
    public interface IZohoCommand
    {
        Task<ZohoSalesOrder> CreateSalesOrder(HSOrderWorksheet orderWorksheet);
        Task<List<ZohoPurchaseOrder>> CreateOrUpdatePurchaseOrder(ZohoSalesOrder z_order, List<HSOrder> orders);
        Task<List<ZohoPurchaseOrder>> CreateShippingPurchaseOrder(ZohoSalesOrder z_order, HSOrderWorksheet order);
        Task<ZohoOrganizationList> ListOrganizations();
    }

    public class ZohoCommand : IZohoCommand
    {
        private const int Delay = 250;
        private const int Concurrent = 1;
        private readonly IZohoClient zoho;
        private readonly IOrderCloudClient oc;
        private readonly AppSettings settings;

        /// <summary>
        /// The IOC based constructor method for the ZohoCommand class object with Dependency Injection
        /// </summary>
        /// <param name="zoho"></param>
        /// <param name="oc"></param>
        /// <param name="settings"></param>
        public ZohoCommand(IZohoClient zoho, IOrderCloudClient oc, AppSettings settings)
        {
            try
            {
                this.settings = settings;
                this.zoho = zoho;
                this.oc = oc;
                this.zoho.AuthenticateAsync();
            }
            catch (Exception ex)
            {
                LoggingNotifications.LogApiResponseMessages(this.settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
                    LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
            }
        }

        /// <summary>
        /// Public re-usable ListOrganizations task method
        /// </summary>
        /// <returns>The ZohoOrganizationList object from the ListOrganizations process</returns>
        public async Task<ZohoOrganizationList> ListOrganizations()
        {
            await zoho.AuthenticateAsync();
            var results = await zoho.Organizations.ListAsync();
            return results;
        }

        /// <summary>
        /// Public re-usable CreateShippingPurchaseOrder task method
        /// Special request by SMG for creating PO of shipments
        /// we definitely don't want this stopping orders from flowing into Zoho so I'm going to handle exceptions and allow to proceed
        /// </summary>
        /// <param name="zOrder"></param>
        /// <param name="order"></param>
        /// <returns>The list of ZohoPurchaseOrder objects from the CreateShippingPurchaseOrder process</returns>
        public async Task<List<ZohoPurchaseOrder>> CreateShippingPurchaseOrder(ZohoSalesOrder z_order, HSOrderWorksheet order)
        {
            // special request by SMG for creating PO of shipments
            // we definitely don't want this stopping orders from flowing into Zoho so I'm going to handle exceptions and allow to proceed
            try
            {
                var list = new List<ZohoPurchaseOrder>();
                foreach (var item in order.ShipEstimateResponse.ShipEstimates)
                {
                    var shipping_method = item.ShipMethods.FirstOrDefault(s => s.ID == item.SelectedShipMethodID);
                    if (shipping_method.xp.CarrierAccountID != "ca_8bdb711131894ab4b42abcd1645d988c")
                    {
                        continue;
                    }

                    var vendor = await zoho.Contacts.ListAsync(new ZohoFilter() { Key = "contact_name", Value = "SMG Shipping" });
                    var oc_lineitems = new ListPage<HSLineItem>()
                    {
                        Items = new List<HSLineItem>()
                        {
                            new HSLineItem()
                            {
                                ID = $"{z_order.reference_number} - {ZohoExtensions.ShippingSuffix}",
                                UnitPrice = shipping_method?.Cost,
                                ProductID = shipping_method.ShippingSku(),
                                SupplierID = "SMG Shipping",
                                Product = new HSLineItemProduct()
                                {
                                    Description = $"{shipping_method?.xp?.Carrier} Shipping Charge",
                                    Name = shipping_method.ShippingSku(),
                                    ID = shipping_method.ShippingSku(),
                                    QuantityMultiplier = 1,
                                    xp = new ProductXp()
                                    {
                                        Tax = new TaxCategorization()
                                        {
                                            Code = "FR",
                                            Description = "Shipping Charge",
                                        },
                                    },
                                },
                            },
                        },
                    };
                    var z_item = await CreateOrUpdateShippingLineItem(oc_lineitems.Items);
                    var oc_order = new Order()
                    {
                        ID = $"{order.Order.ID}-{order.LineItems.FirstOrDefault()?.SupplierID} - 41000",
                        Subtotal = shipping_method.Cost,
                        Total = shipping_method.Cost,
                        TaxCost = 0M,
                    };
                    var oc_lineitem = new ListPage<HSLineItem>() { Items = new List<HSLineItem>() { new HSLineItem() { Quantity = 1 } } };
                    var z_po = ZohoPurchaseOrderMapper.Map(z_order, oc_order, z_item, oc_lineitem.Items.ToList(), null, vendor.Items.FirstOrDefault());
                    var shipping_po = await zoho.PurchaseOrders.ListAsync(new ZohoFilter() { Key = "purchaseorder_number", Value = $"{order.Order.ID}-{order.LineItems.FirstOrDefault()?.SupplierID} - 41000" });
                    if (shipping_po.Items.Any())
                    {
                        z_po.purchaseorder_id = shipping_po.Items.FirstOrDefault()?.purchaseorder_id;
                        list.Add(await zoho.PurchaseOrders.SaveAsync(z_po));
                    }
                    else
                    {
                        list.Add(await zoho.PurchaseOrders.CreateAsync(z_po));
                    }
                }

                return list;
            }
            catch (Exception)
            {
                return new List<ZohoPurchaseOrder>();
            }
        }

        /// <summary>
        /// Public re-usable CreateOrUpdatePurchaseOrder task method
        /// </summary>
        /// <param name="zOrder"></param>
        /// <param name="orders"></param>
        /// <returns>The list of ZohoPurchaseOrder objects from the CreateOrUpdatePurchaseOrder process</returns>
        public async Task<List<ZohoPurchaseOrder>> CreateOrUpdatePurchaseOrder(ZohoSalesOrder z_order, List<HSOrder> orders)
        {
            var results = new List<ZohoPurchaseOrder>();
            foreach (var order in orders)
            {
                var delivery_address = z_order.shipping_address; //TODO: this is not good enough. Might even need to go back to SaleOrder and split out by delivery address
                var supplier = await oc.Suppliers.GetAsync(order.ToCompanyID);

                // TODO: accomodate possibility of more than 100 line items
                var lineitems = await oc.LineItems.ListAllAsync<HSLineItem>(OrderDirection.Outgoing, order.ID);

                // Step 1: Create contact (customer) in Zoho
                var contact = await CreateOrUpdateVendor(order);

                // Step 2: Create or update Items from LineItems/Products on Order
                var items = await CreateOrUpdatePurchaseLineItem(lineitems, supplier);

                // Step 3: Create purchase order
                var po = await CreatePurchaseOrder(z_order, order, items, lineitems, delivery_address, contact);
                results.Add(po);
            }

            return results;
        }

        /// <summary>
        /// Public re-usable CreateSalesOrder task method
        /// </summary>
        /// <param name="orderWorksheet"></param>
        /// <returns>The ZohoSalesOrder object from the CreateSalesOrder process</returns>
        public async Task<ZohoSalesOrder> CreateSalesOrder(HSOrderWorksheet orderWorksheet)
        {
            await zoho.AuthenticateAsync();

            // Step 1: Create contact (customer) in Zoho
            var contact = await CreateOrUpdateContact(orderWorksheet.Order);

            // Step 2: Create or update Items from LineItems/Products on Order
            var items = await CreateOrUpdateSalesLineItems(orderWorksheet.LineItems);

            // Step 3: Create item for shipments
            items.AddRange(await ApplyShipping(orderWorksheet));

            // Step 4: create sales order with all objects from above
            var salesOrder = await CreateSalesOrder(orderWorksheet, items, contact);

            return salesOrder;
        }
		
        /// <summary>
        /// Public re-usable CreatePurchaseOrder task method
        /// </summary>
        /// <param name="zOrder"></param>
        /// <param name="order"></param>
        /// <param name="items"></param>
        /// <param name="lineitems"></param>
        /// <param name="delivery_address"></param>
        /// <param name="contact"></param>
        /// <returns>The ZohoPurchaseOrder object from the CreatePurchaseOrder process</returns>
        private async Task<ZohoPurchaseOrder> CreatePurchaseOrder(ZohoSalesOrder z_order, HSOrder order, List<ZohoLineItem> items, List<HSLineItem> lineitems, ZohoAddress delivery_address, ZohoContact contact)
        {
            var po = await zoho.PurchaseOrders.ListAsync(new ZohoFilter() { Key = "purchaseorder_number", Value = order.ID });
            if (po.Items.Any())
            {
                return await zoho.PurchaseOrders.SaveAsync(ZohoPurchaseOrderMapper.Map(z_order, order, items, lineitems, delivery_address, contact, po.Items.FirstOrDefault()));
            }

            return await zoho.PurchaseOrders.CreateAsync(ZohoPurchaseOrderMapper.Map(z_order, order, items, lineitems, delivery_address, contact));
        }

        /// <summary>
        /// Private re-usable CreateSalesOrder task method
        /// Promotions aren't part of the order worksheet, so we have to get them from OC
        /// </summary>
        /// <param name="orderWorksheet"></param>
        /// <param name="items"></param>
        /// <param name="contact"></param>
        /// <returns>The ZohoSalesOrder object from the CreateSalesOrder process</returns>
        private async Task<ZohoSalesOrder> CreateSalesOrder(HSOrderWorksheet orderWorksheet, IEnumerable<ZohoLineItem> items, ZohoContact contact)
        {
            // promotions aren't part of the order worksheet, so we have to get them from OC
            var promotions = await oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderWorksheet.Order.ID);
            var zOrder = await zoho.SalesOrders.ListAsync(new ZohoFilter() { Key = "reference_number", Value = orderWorksheet.Order.ID });
            if (zOrder.Items.Any())
            {
                return await zoho.SalesOrders.SaveAsync(ZohoSalesOrderMapper.Map(zOrder.Items.FirstOrDefault(), orderWorksheet, items.ToList(), contact, promotions.Items));
            }

            return await zoho.SalesOrders.CreateAsync(ZohoSalesOrderMapper.Map(orderWorksheet, items.ToList(), contact, promotions.Items));
        }

        /// <summary>
        /// Private re-usable PrepareLineItems task method
        /// </summary>
        /// <param name="lineitems"></param>
        /// <returns>The Tuple of Dictionary Items and list of HSLineItem objects from the PrepareLineItems process</returns>
        private async Task<Tuple<Dictionary<string, ZohoLineItem>, List<HSLineItem>>> PrepareLineItems(IList<HSLineItem> lineitems)
        {
            // TODO: accomodate possibility of more than 100 line items
            // Overview: variants will be saved in Zoho as the Item. If the variant is null save the Product as the Item

            // gather IDs either at the product or variant level to search Zoho for existing Items 
            var uniqueLineItems = lineitems.DistinctBy(item => item.SKU()).ToList();

            var zItems = await Throttler.RunAsync(uniqueLineItems, Delay, Concurrent, id => zoho.Items.ListAsync(new ZohoFilter()
            {
                Key = "sku",
                Value = id.SKU(),
            }));
            // the search api returns a list always. if no item was found the list will be empty
            // so we want to get found items into a pared down list
            var z_items = new Dictionary<string, ZohoLineItem>();
            foreach (var list in zItems)
            {
                list.Items.ForEach(item =>
                {
                    if (z_items.Any(z => z.Key == item.sku))
                    {
                        return;
                    }

                    z_items.Add(item.sku, item);
                });
            }

            return new Tuple<Dictionary<string, ZohoLineItem>, List<HSLineItem>>(z_items, uniqueLineItems);
        }

        /// <summary>
        /// Private re-usable CreateOrUpdateShippingLineItem task method
        /// </summary>
        /// <param name="lineitems"></param>
        /// <returns>The list of ZohoLineItem objects from the CreateOrUpdateShippingLineItem process</returns>
        private async Task<List<ZohoLineItem>> CreateOrUpdateShippingLineItem(IList<HSLineItem> lineitems)
        {
            var (z_items, oc_items) = await this.PrepareLineItems(lineitems);

            var items = await Throttler.RunAsync(oc_items, Delay, Concurrent, async lineItem =>
            {
                var (sku, z_item) = z_items.FirstOrDefault(z => z.Key == lineItem.SKU());
                if (z_item != null)
                {
                    return await zoho.Items.SaveAsync(ZohoSalesLineItemMapper.Map(z_item, lineItem));
                }

                return await zoho.Items.CreateAsync(ZohoSalesLineItemMapper.Map(lineItem));
            });
            return items.ToList();
        }

        /// <summary>
        /// Private re-usable CreateOrUpdatePurchaseLineItem task method
        /// </summary>
        /// <param name="lineitems"></param>
        /// <param name="supplier"></param>
        /// <returns>The list of ZohoLineItem objects from the CreateOrUpdatePurchaseLineItem process</returns>
        private async Task<List<ZohoLineItem>> CreateOrUpdatePurchaseLineItem(IList<HSLineItem> lineitems, Supplier supplier)
        {
            var (z_items, oc_items) = await this.PrepareLineItems(lineitems);

            var items = await Throttler.RunAsync(oc_items, Delay, Concurrent, async lineItem =>
            {
                var (sku, z_item) = z_items.FirstOrDefault(z => z.Key == lineItem.SKU());
                if (z_item != null)
                {
                    return await zoho.Items.SaveAsync(ZohoPurchaseLineItemMapper.Map(z_item, lineItem, supplier));
                }

                return await zoho.Items.CreateAsync(ZohoPurchaseLineItemMapper.Map(lineItem, supplier));
            });
            return items.ToList();
        }

        /// <summary>
        /// Private re-usable CreateOrUpdateSalesLineItems task method
        /// </summary>
        /// <param name="lineitems"></param>
        /// <returns>The list of ZohoLineItem objects from the CreateOrUpdateSalesLineItems process</returns>
        private async Task<List<ZohoLineItem>> CreateOrUpdateSalesLineItems(IList<HSLineItem> lineitems)
        {
            var (z_items, oc_items) = await this.PrepareLineItems(lineitems);

            var items = await Throttler.RunAsync(oc_items, Delay, Concurrent, async lineItem =>
            {
                var (sku, z_item) = z_items.FirstOrDefault(z => z.Key == lineItem.SKU());
                if (z_item != null)
                {
                    return await zoho.Items.SaveAsync(ZohoSalesLineItemMapper.Map(z_item, lineItem));
                }

                return await zoho.Items.CreateAsync(ZohoSalesLineItemMapper.Map(lineItem));
            });
            return items.ToList();
        }

        /// <summary>
        /// Private re-usable ApplyShipping task method
        /// </summary>
        /// <param name="orderWorksheet"></param>
        /// <returns>The list of ZohoLineItem objects from the ApplyShipping process</returns>
        private async Task<List<ZohoLineItem>> ApplyShipping(HSOrderWorksheet orderWorksheet)
        {
            var list = new List<ZohoLineItem>();
            if (orderWorksheet.ShipEstimateResponse == null)
            {
                return list;
            }

            foreach (var shipment in orderWorksheet.ShipEstimateResponse.ShipEstimates)
            {
                var method = shipment.ShipMethods.FirstOrDefault(s => s.ID == shipment.SelectedShipMethodID);
                var z_shipping = await zoho.Items.ListAsync(new ZohoFilter() { Key = "sku", Value = method?.ShippingSku() });
                if (z_shipping.Items.Any())
                {
                    list.Add(await zoho.Items.SaveAsync(ZohoShippingLineItemMapper.Map(z_shipping.Items.FirstOrDefault(), method)));
                }
                else
                {
                    list.Add(await zoho.Items.CreateAsync(ZohoShippingLineItemMapper.Map(method)));
                }
            }

            return list;
        }

        /// <summary>
        /// Private re-usable CreateOrUpdateVendor task method
        /// </summary>
        /// <param name="order"></param>
        /// <returns>The ZohoContact object from the CreateOrUpdateVendor process</returns>
        private async Task<ZohoContact> CreateOrUpdateVendor(Order order)
        {
            var supplier = await oc.Suppliers.GetAsync<HSSupplier>(order.ToCompanyID);
            var addresses = await oc.SupplierAddresses.ListAsync<HSAddressSupplier>(order.ToCompanyID);
            var users = await oc.SupplierUsers.ListAsync(order.ToCompanyID);
            var currencies = await zoho.Currencies.ListAsync();
            var vendor = await zoho.Contacts.ListAsync(new ZohoFilter() { Key = "contact_name", Value = supplier.Name });
            if (vendor.Items.Any())
            {
                return await zoho.Contacts.SaveAsync<ZohoContact>(
                    ZohoContactMapper.Map(
                        vendor.Items.FirstOrDefault(),
                        supplier,
                        addresses.Items.FirstOrDefault(),
                        users.Items.FirstOrDefault(),
                        currencies.Items.FirstOrDefault(c => c.currency_code == "USD")));
            }
            else
            {
                return await zoho.Contacts.CreateAsync<ZohoContact>(
                    ZohoContactMapper.Map(
                        supplier,
                        addresses.Items.FirstOrDefault(),
                        users.Items.FirstOrDefault(),
                        currencies.Items.FirstOrDefault(c => c.currency_code == "USD")));
            }
        }

        /// <summary>
        /// Private re-usable CreateOrUpdateContact task method
        /// </summary>
        /// <param name="order"></param>
        /// <returns>The ZohoContact object from the CreateOrUpdateContact process</returns>
        private async Task<ZohoContact> CreateOrUpdateContact(Order order)
        {
            var ocBuyer = await oc.Buyers.GetAsync<HSBuyer>(order.FromCompanyID);
            var buyerAddress = await oc.Addresses.GetAsync<HSAddressBuyer>(order.FromCompanyID, order.BillingAddressID);
            var buyerUserGroup = await oc.UserGroups.GetAsync<HSLocationUserGroup>(order.FromCompanyID, order.BillingAddressID);
            var ocUsers = await oc.Users.ListAsync<HSUser>(ocBuyer.ID, buyerUserGroup.ID);
            var location = new HSBuyerLocation
            {
                Address = buyerAddress,
                UserGroup = buyerUserGroup,
            };

            // TODO: MODEL update ~ eventually add a filter to get the primary contact user
            var currencies = await zoho.Currencies.ListAsync();

            var zContactList = await zoho.Contacts.ListAsync(
                new ZohoFilter() { Key = "contact_name", Value = $"{location.Address?.AddressName} - {location.Address?.xp.LocationID}" },
                new ZohoFilter() { Key = "company_name", Value = $"{ocBuyer.Name} - {location.Address?.xp.LocationID}" });

            var zContact = await zoho.Contacts.GetAsync(zContactList.Items.FirstOrDefault()?.contact_id);
            if (zContact.Item != null)
            {
                var map = ZohoContactMapper.Map(
                    zContact.Item,
                    ocBuyer,
                    ocUsers.Items,
                    currencies.Items.FirstOrDefault(c =>
                        c.currency_code == (location.UserGroup.xp.Currency != null
                            ? location.UserGroup.xp.Currency.ToString()
                            : "USD")),
                    location);
                return await zoho.Contacts.SaveAsync<ZohoContact>(map);
            }

            var contact = ZohoContactMapper.Map(
                ocBuyer,
                ocUsers.Items,
                currencies.Items.FirstOrDefault(c =>
                    c.currency_code == (location.UserGroup.xp.Currency != null
                        ? location.UserGroup.xp.Currency.ToString()
                        : "USD")),
                location);
            return await zoho.Contacts.CreateAsync<ZohoContact>(contact);
        }
    }
}
