using System;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Models;
using OrderCloud.Catalyst;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;
using Headstart.Models.Headstart;
using Headstart.Common.Services.Zoho;
using ordercloud.integrations.library;
using Headstart.Common.Services.Zoho.Models;
using Headstart.Common.Services.Zoho.Mappers;
using ordercloud.integrations.library.intefaces;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using Headstart.Common.Services.ShippingIntegration.Models;
using Sitecore.Foundation.SitecoreExtensions.MVC.Extenstions;

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
        private readonly IZohoClient _zoho;
        private readonly IOrderCloudClient _oc;
        private const int delay = 250;
        private const int concurrent = 1;
        private WebConfigSettings _webConfigSettings = WebConfigSettings.Instance;

        /// <summary>
        /// The IOC based constructor method for the ZohoCommand class object with Dependency Injection
        /// </summary>
        /// <param name="zoho"></param>
        /// <param name="oc"></param>
        public ZohoCommand(IZohoClient zoho, IOrderCloudClient oc)
        {
            try
            {
                _zoho = zoho;
                _oc = oc;
                _zoho.AuthenticateAsync();
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
        }

        /// <summary>
        /// Public re-usable ListOrganizations task method
        /// </summary>
        /// <returns>The ZohoOrganizationList response object from the ListOrganizations process</returns>
        public async Task<ZohoOrganizationList> ListOrganizations()
        {
            var results = new ZohoOrganizationList();
            try
            {
                await _zoho.AuthenticateAsync();
                results = await _zoho.Organizations.ListAsync();
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return results;
        }

        /// <summary>
        /// Public re-usable CreateShippingPurchaseOrder task method
        /// Special request by SMG for creating PO of shipments
        /// we definitely don't want this stopping orders from flowing into Zoho so I'm going to handle exceptions and allow to proceed
        /// </summary>
        /// <param name="z_order"></param>
        /// <param name="order"></param>
        /// <returns>The list of ZohoPurchaseOrder response objects from the CreateShippingPurchaseOrder process</returns>
        public async Task<List<ZohoPurchaseOrder>> CreateShippingPurchaseOrder(ZohoSalesOrder z_order, HSOrderWorksheet order)
        {
            var list = new List<ZohoPurchaseOrder>();
            try
            {
                foreach (var item in order.ShipEstimateResponse.ShipEstimates)
                {
                    var shipping_method = item.ShipMethods.FirstOrDefault(s => s.ID == item.SelectedShipMethodID);
                    if ((!shipping_method.xp.CarrierAccountID.Equals("ca_8bdb711131894ab4b42abcd1645d988c", StringComparison.OrdinalIgnoreCase))) 
                    { 
                        continue; 
                    }

                    var vendor = await _zoho.Contacts.ListAsync(new ZohoFilter() { Key = "contact_name", Value = "SMG Shipping" });
                    var oc_lineitems = new ListPage<HSLineItem>()
                    {
                        Items = new List<HSLineItem>()
                        {
                            new HSLineItem() {
                                ID = $@"{z_order.reference_number} - {ZohoExtensions.ShippingSuffix}",
                                UnitPrice = shipping_method?.Cost,
                                ProductID = shipping_method.ShippingSku(),
                                SupplierID = $@"SMG Shipping",
                                Product = new HSLineItemProduct()
                                {
                                    Description = $@"{shipping_method?.xp?.Carrier} Shipping Charge",
                                    Name = shipping_method.ShippingSku(),
                                    ID = shipping_method.ShippingSku(),
                                    QuantityMultiplier = 1,
                                    xp = new ProductXp()
                                    {
                                        Tax = new TaxCategorization()
                                        {
                                            Code = $@"FR",
                                            Description = $@"Shipping Charge"
                                        }
                                    }
                                }
                            }
                        }
                    };

                    var z_item = await CreateOrUpdateShippingLineItem(oc_lineitems.Items);
                    var oc_order = new Order()
                    {
                        ID = $@"{order.Order.ID}-{order.LineItems.FirstOrDefault()?.SupplierID} - 41000",
                        Subtotal = shipping_method.Cost,
                        Total = shipping_method.Cost,
                        TaxCost = 0M
                    };
                    var oc_lineitem = new ListPage<HSLineItem>() { Items = new List<HSLineItem>() { new HSLineItem() { Quantity = 1 } } };
                    var z_po = ZohoPurchaseOrderMapper.Map(z_order, oc_order, z_item, oc_lineitem.Items.ToList(), null, vendor.Items.FirstOrDefault());
                    var shipping_po = await _zoho.PurchaseOrders.ListAsync(new ZohoFilter() { Key = $@"purchaseorder_number", Value = $@"{order.Order.ID}-{order.LineItems.FirstOrDefault()?.SupplierID} - 41000" });
                    if (shipping_po.Items.Any())
                    {
                        z_po.purchaseorder_id = shipping_po.Items.FirstOrDefault()?.purchaseorder_id;
                        list.Add(await _zoho.PurchaseOrders.SaveAsync(z_po));
                    }
                    else
                    {
                        list.Add(await _zoho.PurchaseOrders.CreateAsync(z_po));
                    }
                }
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return list;
        }

        /// <summary>
        /// Public re-usable CreateOrUpdatePurchaseOrder task method
        /// </summary>
        /// <param name="z_order"></param>
        /// <param name="orders"></param>
        /// <returns>The list of ZohoPurchaseOrder response objects from the CreateOrUpdatePurchaseOrder process</returns>
        public async Task<List<ZohoPurchaseOrder>> CreateOrUpdatePurchaseOrder(ZohoSalesOrder z_order, List<HSOrder> orders)
        {
            var results = new List<ZohoPurchaseOrder>();
            try
            {
                foreach (var order in orders)
                {
                    var delivery_address = z_order.shipping_address; //TODO: this is not good enough. Might even need to go back to SaleOrder and split out by delivery address
                    var supplier = await _oc.Suppliers.GetAsync(order.ToCompanyID);
                    // TODO: accomodate possibility of more than 100 line items
                    var lineitems = await _oc.LineItems.ListAllAsync<HSLineItem>(OrderDirection.Outgoing, order.ID);
                    // Step 1: Create contact (customer) in Zoho
                    var contact = await CreateOrUpdateVendor(order);
                    // Step 2: Create or update Items from LineItems/Products on Order
                    var items = await CreateOrUpdatePurchaseLineItem(lineitems, supplier);
                    // Step 3: Create purchase order
                    var po = await CreatePurchaseOrder(z_order, order, items, lineitems, delivery_address, contact);
                    results.Add(po);
                }
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return results;
        }

        /// <summary>
        /// Public re-usable CreatePurchaseOrder task method
        /// </summary>
        /// <param name="z_order"></param>
        /// <param name="order"></param>
        /// <param name="items"></param>
        /// <param name="lineitems"></param>
        /// <param name="delivery_address"></param>
        /// <param name="contact"></param>
        /// <returns>The ZohoPurchaseOrder response object from the CreatePurchaseOrder process</returns>
        private async Task<ZohoPurchaseOrder> CreatePurchaseOrder(ZohoSalesOrder z_order, HSOrder order, List<ZohoLineItem> items, List<HSLineItem> lineitems, ZohoAddress delivery_address, ZohoContact contact)
        {
            var resp = new ZohoPurchaseOrder();
            try
            {
                var po = await _zoho.PurchaseOrders.ListAsync(new ZohoFilter() { Key = "purchaseorder_number", Value = order.ID });
                if (po.Items.Any())
                {
                    resp = await _zoho.PurchaseOrders.SaveAsync(ZohoPurchaseOrderMapper.Map(z_order, order, items, lineitems, delivery_address, contact, po.Items.FirstOrDefault()));
                }
                else
                {
                    resp = await _zoho.PurchaseOrders.CreateAsync(ZohoPurchaseOrderMapper.Map(z_order, order, items, lineitems, delivery_address, contact));
                }
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Public re-usable CreateSalesOrder task method
        /// </summary>
        /// <param name="orderWorksheet"></param>
        /// <returns>The ZohoSalesOrder response object from the CreateSalesOrder process</returns>
        public async Task<ZohoSalesOrder> CreateSalesOrder(HSOrderWorksheet orderWorksheet)
        {
            var salesOrder = new ZohoSalesOrder();
            try
            {
                await _zoho.AuthenticateAsync();
                // Step 1: Create contact (customer) in Zoho
                var contact = await CreateOrUpdateContact(orderWorksheet.Order);
                // Step 2: Create or update Items from LineItems/Products on Order
                var items = await CreateOrUpdateSalesLineItems(orderWorksheet.LineItems);
                // Step 3: Create item for shipments
                items.AddRange(await ApplyShipping(orderWorksheet));
                // Step 4: create sales order with all objects from above
                salesOrder = await CreateSalesOrder(orderWorksheet, items, contact);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return salesOrder;
        }

        /// <summary>
        /// Private re-usable CreateSalesOrder task method
        /// Promotions aren't part of the order worksheet, so we have to get them from OC
        /// </summary>
        /// <param name="orderWorksheet"></param>
        /// <param name="items"></param>
        /// <param name="contact"></param>
        /// <returns>The ZohoSalesOrder response object from the CreateSalesOrder process</returns>
        private async Task<ZohoSalesOrder> CreateSalesOrder(HSOrderWorksheet orderWorksheet, IEnumerable<ZohoLineItem> items, ZohoContact contact)
        {
            var resp = new ZohoSalesOrder();
            try
            {
                var promotions = await _oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderWorksheet.Order.ID);
                var zOrder = await _zoho.SalesOrders.ListAsync(new ZohoFilter() { Key = $@"reference_number", Value = orderWorksheet.Order.ID });
                if (zOrder.Items.Any())
                {
                    resp = await _zoho.SalesOrders.SaveAsync(ZohoSalesOrderMapper.Map(zOrder.Items.FirstOrDefault(), orderWorksheet, items.ToList(), contact, promotions.Items));
                }
                else
                {
                    resp = await _zoho.SalesOrders.CreateAsync(ZohoSalesOrderMapper.Map(orderWorksheet, items.ToList(), contact, promotions.Items));
                }
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Private re-usable PrepareLineItems task method
        /// </summary>
        /// <param name="lineitems"></param>
        /// <returns>The Tuple of Dictionary Items and list of HSLineItem response objects from the PrepareLineItems process</returns>
        private async Task<Tuple<Dictionary<string, ZohoLineItem>, List<HSLineItem>>> PrepareLineItems(IList<HSLineItem> lineitems)
        {
            var resp = new Tuple<Dictionary<string, ZohoLineItem>, List<HSLineItem>>( new Dictionary<string, ZohoLineItem>(), new List<HSLineItem>());
            try
            {
                // TODO: accomodate possibility of more than 100 line items
                // Overview: variants will be saved in Zoho as the Item. If the variant is null save the Product as the Item
                // gather IDs either at the product or variant level to search Zoho for existing Items 
                var uniqueLineItems = lineitems.DistinctBy(item => item.SKU()).ToList();

                var zItems = await Throttler.RunAsync(uniqueLineItems, delay, concurrent, id => _zoho.Items.ListAsync(new ZohoFilter()
                {
                    Key = $@"sku",
                    Value = id.SKU()
                }));
                // the search api returns a list always. if no item was found the list will be empty
                // so we want to get found items into a pared down list
                var z_items = new Dictionary<string, ZohoLineItem>();
                foreach (var list in zItems)
                {
                    list.Items.ForEach(item =>
                    {
                        if (z_items.Any(z => z.Key == item.sku)) return;
                        z_items.Add(item.sku, item);
                    });
                }
                resp = new Tuple<Dictionary<string, ZohoLineItem>, List<HSLineItem>>(z_items, uniqueLineItems);
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Private re-usable CreateOrUpdateShippingLineItem task method
        /// </summary>
        /// <param name="lineitems"></param>
        /// <returns>The list of ZohoLineItem response objects from the CreateOrUpdateShippingLineItem process</returns>
        private async Task<List<ZohoLineItem>> CreateOrUpdateShippingLineItem(IList<HSLineItem> lineitems)
        {
            var resp = new List<ZohoLineItem>();
            try
            {
                var (z_items, oc_items) = await this.PrepareLineItems(lineitems);
                var items = await Throttler.RunAsync(oc_items, delay, concurrent, async lineItem =>
                {
                    var (sku, z_item) = z_items.FirstOrDefault(z => z.Key == lineItem.SKU());
                    if (z_item != null)
                    {
                        return await _zoho.Items.SaveAsync(ZohoSalesLineItemMapper.Map(z_item, lineItem));
                    }
                    return await _zoho.Items.CreateAsync(ZohoSalesLineItemMapper.Map(lineItem));
                });
                resp = items.ToList();
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Private re-usable CreateOrUpdatePurchaseLineItem task method
        /// </summary>
        /// <param name="lineitems"></param>
        /// <param name="supplier"></param>
        /// <returns>The list of ZohoLineItem response objects from the CreateOrUpdatePurchaseLineItem process</returns>
        private async Task<List<ZohoLineItem>> CreateOrUpdatePurchaseLineItem(IList<HSLineItem> lineitems, Supplier supplier)
        {
            var resp = new List<ZohoLineItem>();
            try
            {
                var (z_items, oc_items) = await this.PrepareLineItems(lineitems);
                var items = await Throttler.RunAsync(oc_items, delay, concurrent, async lineItem =>
                {
                    var (sku, z_item) = z_items.FirstOrDefault(z => z.Key == lineItem.SKU());
                    if (z_item != null)
                    {
                        return await _zoho.Items.SaveAsync(ZohoPurchaseLineItemMapper.Map(z_item, lineItem, supplier));
                    }
                    return await _zoho.Items.CreateAsync(ZohoPurchaseLineItemMapper.Map(lineItem, supplier));
                });
                resp = items.ToList();
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Private re-usable CreateOrUpdateSalesLineItems task method
        /// </summary>
        /// <param name="lineitems"></param>
        /// <returns>The list of ZohoLineItem response objects from the CreateOrUpdateSalesLineItems process</returns>
        private async Task<List<ZohoLineItem>> CreateOrUpdateSalesLineItems(IList<HSLineItem> lineitems)
        {
            var resp = new List<ZohoLineItem>();
            try
            {
                var (z_items, oc_items) = await this.PrepareLineItems(lineitems);
                var items = await Throttler.RunAsync(oc_items, delay, concurrent, async lineItem =>
                {
                    var (sku, z_item) = z_items.FirstOrDefault(z => z.Key == lineItem.SKU());
                    if (z_item != null)
                    {
                        return await _zoho.Items.SaveAsync(ZohoSalesLineItemMapper.Map(z_item, lineItem));
                    }
                    return await _zoho.Items.CreateAsync(ZohoSalesLineItemMapper.Map(lineItem));
                });
                resp = items.ToList();
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Private re-usable ApplyShipping task method
        /// </summary>
        /// <param name="orderWorksheet"></param>
        /// <returns>The list of ZohoLineItem response objects from the ApplyShipping process</returns>
        private async Task<List<ZohoLineItem>> ApplyShipping(HSOrderWorksheet orderWorksheet)
        {
            var list = new List<ZohoLineItem>();
            try
            {
                if (orderWorksheet.ShipEstimateResponse == null)
                {
                    return list;
                }
                foreach (var shipment in orderWorksheet.ShipEstimateResponse.ShipEstimates)
                {
                    var method = shipment.ShipMethods.FirstOrDefault(s => s.ID == shipment.SelectedShipMethodID);
                    var z_shipping = await _zoho.Items.ListAsync(new ZohoFilter() { Key = "sku", Value = method?.ShippingSku() });
                    if (z_shipping.Items.Any())
                    {
                        list.Add(await _zoho.Items.SaveAsync(ZohoShippingLineItemMapper.Map(z_shipping.Items.FirstOrDefault(), method)));
                    }
                    else
                    {
                        list.Add(await _zoho.Items.CreateAsync(ZohoShippingLineItemMapper.Map(method)));
                    }
                }
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return list;
        }

        /// <summary>
        /// Private re-usable CreateOrUpdateVendor task method
        /// </summary>
        /// <param name="order"></param>
        /// <returns>The ZohoContact response object from the CreateOrUpdateVendor process</returns>
        private async Task<ZohoContact> CreateOrUpdateVendor(Order order)
        {
            var resp = new ZohoContact();
            try
            {
                var supplier = await _oc.Suppliers.GetAsync<HSSupplier>(order.ToCompanyID);
                var addresses = await _oc.SupplierAddresses.ListAsync<HSAddressSupplier>(order.ToCompanyID);
                var users = await _oc.SupplierUsers.ListAsync(order.ToCompanyID);
                var currencies = await _zoho.Currencies.ListAsync();
                var vendor = await _zoho.Contacts.ListAsync(new ZohoFilter() { Key = $@"contact_name", Value = supplier.Name });
                if (vendor.Items.Any())
                {
                    resp = await _zoho.Contacts.SaveAsync<ZohoContact>(ZohoContactMapper.Map(vendor.Items.FirstOrDefault(), supplier, addresses.Items.FirstOrDefault(), users.Items.FirstOrDefault(),
                            currencies.Items.FirstOrDefault(c => c.currency_code.Equals($@"USD", StringComparison.OrdinalIgnoreCase))));
                }
                else
                {
                    resp = await _zoho.Contacts.CreateAsync<ZohoContact>(ZohoContactMapper.Map(supplier, addresses.Items.FirstOrDefault(), users.Items.FirstOrDefault(),
                            currencies.Items.FirstOrDefault(c => c.currency_code.Equals($@"USD", StringComparison.OrdinalIgnoreCase))));
                }
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }

        /// <summary>
        /// Private re-usable CreateOrUpdateContact task method
        /// </summary>
        /// <param name="order"></param>
        /// <returns>The ZohoContact response object from the CreateOrUpdateContact process</returns>
        private async Task<ZohoContact> CreateOrUpdateContact(Order order)
        {
            var resp = new ZohoContact();
            try
            {
                var ocBuyer = await _oc.Buyers.GetAsync<HSBuyer>(order.FromCompanyID);
                var buyerAddress = await _oc.Addresses.GetAsync<HSAddressBuyer>(order.FromCompanyID, order.BillingAddressID);
                var buyerUserGroup = await _oc.UserGroups.GetAsync<HSLocationUserGroup>(order.FromCompanyID, order.BillingAddressID);
                var ocUsers = await _oc.Users.ListAsync<HSUser>(ocBuyer.ID, buyerUserGroup.ID);
                var location = new HSBuyerLocation
                {
                    Address = buyerAddress,
                    UserGroup = buyerUserGroup
                };

                // TODO: MODEL update ~ eventually add a filter to get the primary contact user
                var currencies = await _zoho.Currencies.ListAsync();
                var zContactList = await _zoho.Contacts.ListAsync(new ZohoFilter() { Key = $@"contact_name", Value = $@"{location.Address?.AddressName} - {location.Address?.xp.LocationID}" },
                    new ZohoFilter() { Key = $@"company_name", Value = $@"{ocBuyer.Name} - {location.Address?.xp.LocationID}" });
                var zContact = await _zoho.Contacts.GetAsync(zContactList.Items.FirstOrDefault()?.contact_id);
                if (zContact.Item != null)
                {
                    var map = ZohoContactMapper.Map(zContact.Item, ocBuyer, ocUsers.Items,
                        currencies.Items.FirstOrDefault(c => c.currency_code == (location.UserGroup.xp.Currency != null ? location.UserGroup.xp.Currency.ToString() : "USD")), location);
                    resp = await _zoho.Contacts.SaveAsync<ZohoContact>(map);
                }
                else
                {
                    var contact = ZohoContactMapper.Map(ocBuyer, ocUsers.Items, currencies.Items.FirstOrDefault(c => c.currency_code == (location.UserGroup.xp.Currency != null
                        ? location.UserGroup.xp.Currency.ToString() : "USD")), location);
                    resp = await _zoho.Contacts.CreateAsync<ZohoContact>(contact);
                }
            }
            catch (Exception ex)
            {
                LogExt.LogException(_webConfigSettings.AppLogFileKey, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
            }
            return resp;
        }
    }
}