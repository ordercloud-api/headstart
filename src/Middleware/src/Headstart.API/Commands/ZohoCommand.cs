using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Common.Services.Zoho;
using Headstart.Common.Services.Zoho.Mappers;
using Headstart.Common.Services.Zoho.Models;
using Headstart.Models;
using Headstart.Models.Extended;
using Headstart.Models.Headstart;
using ordercloud.integrations.library;
using ordercloud.integrations.library.intefaces;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

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

        public ZohoCommand(IZohoClient zoho, IOrderCloudClient oc)
        {
            _zoho = zoho;
            _oc = oc;
            _zoho.AuthenticateAsync();
        }
        
        public async Task<ZohoOrganizationList> ListOrganizations()
        {
            await _zoho.AuthenticateAsync();
            var results = await _zoho.Organizations.ListAsync();
            return results;
        }

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
                    if (shipping_method.xp.CarrierAccountID != "ca_8bdb711131894ab4b42abcd1645d988c") continue;
                    var vendor = await _zoho.Contacts.ListAsync(new ZohoFilter() { Key = "contact_name", Value = "SMG Shipping" });
                    var oc_lineitems = new ListPage<HSLineItem>()
                    {
                        Items = new List<HSLineItem>()
                        {
                            new HSLineItem() {
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
                                            Description = "Shipping Charge"
                                        }
                                    }
                                }
                            }
                        }
                    };
                    var z_item = await CreateOrUpdateShippingLineItem(oc_lineitems.Items);
                    var oc_order = new Order()
                    {
                        ID = $"{order.Order.ID}-{order.LineItems.FirstOrDefault()?.SupplierID} - 41000",
                        Subtotal = shipping_method.Cost,
                        Total = shipping_method.Cost,
                        TaxCost = 0M
                    };
                    var oc_lineitem = new ListPage<HSLineItem>() { Items = new List<HSLineItem>() { new HSLineItem() { Quantity = 1 } } };
                    var z_po = ZohoPurchaseOrderMapper.Map(z_order, oc_order, z_item, oc_lineitem.Items.ToList(), null, vendor.Items.FirstOrDefault());
                    var shipping_po = await _zoho.PurchaseOrders.ListAsync(new ZohoFilter() { Key = "purchaseorder_number", Value = $"{order.Order.ID}-{order.LineItems.FirstOrDefault()?.SupplierID} - 41000" });
                    if (shipping_po.Items.Any())
                    {
                        z_po.purchaseorder_id = shipping_po.Items.FirstOrDefault()?.purchaseorder_id;
                        list.Add(await _zoho.PurchaseOrders.SaveAsync(z_po));
                    }
                    else
                        list.Add(await _zoho.PurchaseOrders.CreateAsync(z_po));
                }

                return list;
            }
            catch (Exception ex)
            {
                return new List<ZohoPurchaseOrder>();
            }
        }

        public async Task<List<ZohoPurchaseOrder>> CreateOrUpdatePurchaseOrder(ZohoSalesOrder z_order, List<HSOrder> orders)
        {
            var results = new List<ZohoPurchaseOrder>();
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

            return results;
        }

        private async Task<ZohoPurchaseOrder> CreatePurchaseOrder(ZohoSalesOrder z_order, HSOrder order, List<ZohoLineItem> items, List<HSLineItem> lineitems, ZohoAddress delivery_address, ZohoContact contact)
        {
            var po = await _zoho.PurchaseOrders.ListAsync(new ZohoFilter() { Key = "purchaseorder_number", Value = order.ID });
            if (po.Items.Any())
                return await _zoho.PurchaseOrders.SaveAsync(ZohoPurchaseOrderMapper.Map(z_order, order, items, lineitems, delivery_address, contact, po.Items.FirstOrDefault()));
            return await _zoho.PurchaseOrders.CreateAsync(ZohoPurchaseOrderMapper.Map(z_order, order, items, lineitems, delivery_address, contact));
        }

        public async Task<ZohoSalesOrder> CreateSalesOrder(HSOrderWorksheet orderWorksheet)
        {
            await _zoho.AuthenticateAsync();
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

        private async Task<ZohoSalesOrder> CreateSalesOrder(HSOrderWorksheet orderWorksheet, IEnumerable<ZohoLineItem> items, ZohoContact contact)
        {
            // promotions aren't part of the order worksheet, so we have to get them from OC
            var promotions = await _oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderWorksheet.Order.ID);
            var zOrder = await _zoho.SalesOrders.ListAsync(new ZohoFilter() { Key = "reference_number", Value = orderWorksheet.Order.ID });
            if (zOrder.Items.Any())
                return await _zoho.SalesOrders.SaveAsync(ZohoSalesOrderMapper.Map(zOrder.Items.FirstOrDefault(), orderWorksheet, items.ToList(), contact, promotions.Items));
            return await _zoho.SalesOrders.CreateAsync(ZohoSalesOrderMapper.Map(orderWorksheet, items.ToList(), contact, promotions.Items));
        }

        private async Task<Tuple<Dictionary<string, ZohoLineItem>, List<HSLineItem>>> PrepareLineItems(IList<HSLineItem> lineitems)
        {
            // TODO: accomodate possibility of more than 100 line items
            // Overview: variants will be saved in Zoho as the Item. If the variant is null save the Product as the Item

            // gather IDs either at the product or variant level to search Zoho for existing Items 
            var uniqueLineItems = lineitems.DistinctBy(item => item.SKU()).ToList();

            var zItems = await Throttler.RunAsync(uniqueLineItems, delay, concurrent, id => _zoho.Items.ListAsync(new ZohoFilter()
            {
                Key = "sku",
                Value = id.SKU()
            }));
            // the search api returns a list always. if no item was found the list will be empty
            // so we want to get found items into a pared down list
            var z_items = new Dictionary<string, ZohoLineItem>();
            foreach (var list in zItems)
                list.Items.ForEach(item =>
                {
                    if (z_items.Any(z => z.Key == item.sku)) return;
                    z_items.Add(item.sku, item);
                });
            return new Tuple<Dictionary<string, ZohoLineItem>, List<HSLineItem>>(z_items, uniqueLineItems);
        }

        private async Task<List<ZohoLineItem>> CreateOrUpdateShippingLineItem(IList<HSLineItem> lineitems)
        {
            var (z_items, oc_items) = await this.PrepareLineItems(lineitems);

            var items = await Throttler.RunAsync(oc_items, delay, concurrent, async lineItem =>
            {
                var (sku, z_item) = z_items.FirstOrDefault(z => z.Key == lineItem.SKU());
                if (z_item != null)
                    return await _zoho.Items.SaveAsync(ZohoSalesLineItemMapper.Map(z_item, lineItem));
                return await _zoho.Items.CreateAsync(ZohoSalesLineItemMapper.Map(lineItem));
            });
            return items.ToList();
        }

        private async Task<List<ZohoLineItem>> CreateOrUpdatePurchaseLineItem(IList<HSLineItem> lineitems, Supplier supplier)
        {
            var (z_items, oc_items) = await this.PrepareLineItems(lineitems);

            var items = await Throttler.RunAsync(oc_items, delay, concurrent, async lineItem =>
            {
                var (sku, z_item) = z_items.FirstOrDefault(z => z.Key == lineItem.SKU());
                if (z_item != null)
                    return await _zoho.Items.SaveAsync(ZohoPurchaseLineItemMapper.Map(z_item, lineItem, supplier));
                return await _zoho.Items.CreateAsync(ZohoPurchaseLineItemMapper.Map(lineItem, supplier));
            });
            return items.ToList();
        }

        private async Task<List<ZohoLineItem>> CreateOrUpdateSalesLineItems(IList<HSLineItem> lineitems)
        {
            var (z_items, oc_items) = await this.PrepareLineItems(lineitems);

            var items = await Throttler.RunAsync(oc_items, delay, concurrent, async lineItem =>
            {
                var (sku, z_item) = z_items.FirstOrDefault(z => z.Key == lineItem.SKU());
                if (z_item != null)
                    return await _zoho.Items.SaveAsync(ZohoSalesLineItemMapper.Map(z_item, lineItem));
                return await _zoho.Items.CreateAsync(ZohoSalesLineItemMapper.Map(lineItem));
            });
            return items.ToList();
        }

        private async Task<List<ZohoLineItem>> ApplyShipping(HSOrderWorksheet orderWorksheet)
        {
            var list = new List<ZohoLineItem>();
            if (orderWorksheet.ShipEstimateResponse == null) return list;
            foreach (var shipment in orderWorksheet.ShipEstimateResponse.ShipEstimates)
            {
                var method = shipment.ShipMethods.FirstOrDefault(s => s.ID == shipment.SelectedShipMethodID);
                var z_shipping = await _zoho.Items.ListAsync(new ZohoFilter() { Key = "sku", Value = method?.ShippingSku() });
                if (z_shipping.Items.Any())
                    list.Add(await _zoho.Items.SaveAsync(ZohoShippingLineItemMapper.Map(z_shipping.Items.FirstOrDefault(), method)));
                else
                    list.Add(await _zoho.Items.CreateAsync(ZohoShippingLineItemMapper.Map(method)));
            }

            return list;
        }

        private async Task<ZohoContact> CreateOrUpdateVendor(Order order)
        {
            var supplier = await _oc.Suppliers.GetAsync<HSSupplier>(order.ToCompanyID);
            var addresses = await _oc.SupplierAddresses.ListAsync<HSAddressSupplier>(order.ToCompanyID);
            var users = await _oc.SupplierUsers.ListAsync(order.ToCompanyID);
            var currencies = await _zoho.Currencies.ListAsync();
            var vendor = await _zoho.Contacts.ListAsync(new ZohoFilter() { Key = "contact_name", Value = supplier.Name });
            if (vendor.Items.Any())
            {
                return await _zoho.Contacts.SaveAsync<ZohoContact>(
                    ZohoContactMapper.Map(
                        vendor.Items.FirstOrDefault(),
                        supplier,
                        addresses.Items.FirstOrDefault(),
                        users.Items.FirstOrDefault(),
                        currencies.Items.FirstOrDefault(c => c.currency_code == "USD")));
            }
            else
            {
                return await _zoho.Contacts.CreateAsync<ZohoContact>(
                    ZohoContactMapper.Map(
                        supplier,
                        addresses.Items.FirstOrDefault(),
                        users.Items.FirstOrDefault(),
                        currencies.Items.FirstOrDefault(c => c.currency_code == "USD")));
            }
        }

        private async Task<ZohoContact> CreateOrUpdateContact(Order order)
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

            var zContactList = await _zoho.Contacts.ListAsync(
                new ZohoFilter() { Key = "contact_name", Value = $"{location.Address?.AddressName} - {location.Address?.xp.LocationID}" },
                new ZohoFilter() { Key = "company_name", Value = $"{ocBuyer.Name} - {location.Address?.xp.LocationID}" });

            var zContact = await _zoho.Contacts.GetAsync(zContactList.Items.FirstOrDefault()?.contact_id);
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
                return await _zoho.Contacts.SaveAsync<ZohoContact>(map);
            }
            var contact = ZohoContactMapper.Map(
                ocBuyer,
                ocUsers.Items,
                currencies.Items.FirstOrDefault(c =>
                    c.currency_code == (location.UserGroup.xp.Currency != null
                        ? location.UserGroup.xp.Currency.ToString()
                        : "USD")),
                location);
            return await _zoho.Contacts.CreateAsync<ZohoContact>(contact);
        }
    }
}