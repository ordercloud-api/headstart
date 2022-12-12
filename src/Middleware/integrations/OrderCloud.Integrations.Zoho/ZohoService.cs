using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Models;
using Headstart.Common.Services;
using Headstart.Common.Utils;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Zoho.Mappers;
using OrderCloud.Integrations.Zoho.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Zoho
{
    public class ZohoService : IOMSService
    {
        private const int Delay = 250;
        private const int Concurrent = 1;
        private readonly IZohoClient zoho;
        private readonly IOrderCloudClient oc;

        public ZohoService(IZohoClient zoho, IOrderCloudClient oc)
        {
            this.zoho = zoho;
            this.oc = oc;
            this.zoho.AuthenticateAsync();
        }

        public async Task<ProcessResult> ExportOrder(HSOrderWorksheet worksheet, IList<HSOrder> supplierOrders, bool isOrderSubmit = true)
        {
            if (isOrderSubmit && !zoho.Config.PerformOrderSubmitTasks)
            {
                return null;
            }

            var (salesAction, zohoSalesOrder) = await ProcessAction.Execute(
                ProcessType.Accounting,
                "Create Zoho Sales Order",
                CreateSalesOrder(worksheet));

            var (poAction, zohoPurchaseOrder) = await ProcessAction.Execute(
                ProcessType.Accounting,
                "Create Zoho Purchase Order",
                CreateOrUpdatePurchaseOrder(zohoSalesOrder, supplierOrders.ToList()));

            return new ProcessResult()
            {
                Type = ProcessType.Accounting,
                Activity = new List<ProcessResultAction>() { salesAction, poAction },
            };
        }

        private async Task<ZohoOrganizationList> ListOrganizations()
        {
            await zoho.AuthenticateAsync();
            var results = await zoho.Organizations.ListAsync();
            return results;
        }

        private async Task<List<ZohoPurchaseOrder>> CreateOrUpdatePurchaseOrder(ZohoSalesOrder z_order, List<HSOrder> orders)
        {
            var results = new List<ZohoPurchaseOrder>();
            foreach (var order in orders)
            {
                var delivery_address = z_order.shipping_address; // TODO: this is not good enough. Might even need to go back to SaleOrder and split out by delivery address
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

        private async Task<ZohoSalesOrder> CreateSalesOrder(HSOrderWorksheet orderWorksheet)
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

        private async Task<ZohoPurchaseOrder> CreatePurchaseOrder(ZohoSalesOrder z_order, HSOrder order, List<ZohoLineItem> items, List<HSLineItem> lineitems, ZohoAddress delivery_address, ZohoContact contact)
        {
            var po = await zoho.PurchaseOrders.ListAsync(new ZohoFilter() { Key = "purchaseorder_number", Value = order.ID });
            if (po.Items.Any())
            {
                return await zoho.PurchaseOrders.SaveAsync(ZohoPurchaseOrderMapper.Map(z_order, order, items, lineitems, delivery_address, contact, po.Items.FirstOrDefault()));
            }

            return await zoho.PurchaseOrders.CreateAsync(ZohoPurchaseOrderMapper.Map(z_order, order, items, lineitems, delivery_address, contact));
        }

        private async Task<ZohoSalesOrder> CreateSalesOrder(HSOrderWorksheet orderWorksheet, IEnumerable<ZohoLineItem> items, ZohoContact contact)
        {
            // promotions aren't part of the order worksheet, so we have to get them from OC
            var promotions = await oc.Orders.ListPromotionsAsync(OrderDirection.Incoming, orderWorksheet.Order.ID);
            var zOrder = await zoho.SalesOrders.ListAsync(new ZohoFilter() { Key = "reference_number", Value = orderWorksheet.Order.ID });
            if (zOrder.Items.Any())
            {
                return await zoho.SalesOrders.SaveAsync(ZohoSalesOrderMapper.Map(orderWorksheet, items.ToList(), contact, promotions.Items, zOrder.Items.FirstOrDefault()));
            }

            return await zoho.SalesOrders.CreateAsync(ZohoSalesOrderMapper.Map(orderWorksheet, items.ToList(), contact, promotions.Items));
        }

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
