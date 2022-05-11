using System;
using System.Collections.Generic;
using System.Linq;
using Headstart.Common.Services.ShippingIntegration.Models;
using Headstart.Common.Services.Zoho.Models;
using Headstart.Models;
using Headstart.Models.Headstart;
using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace Headstart.Common.Services.Zoho.Mappers
{
    public static class ZohoContactMapper
    {
        public static ZohoContact Map(HSSupplier supplier, HSAddressSupplier address, User user,
            ZohoCurrency currency)
        {
            return new ZohoContact()
            {
                company_name = supplier.ID,
                contact_name = supplier.Name,
                contact_type = "vendor",
                billing_address = ZohoAddressMapper.Map(address),
                shipping_address = ZohoAddressMapper.Map(address),
                contact_persons = new List<ZohoContactPerson>()
                {
                    new ZohoContactPerson()
                    {
                        email = user.Email,
                        first_name = user.FirstName,
                        last_name = user.LastName,
                        phone = user.Phone
                    }
                },
                currency_id = currency.currency_id
            };
        }

        public static ZohoContact Map(ZohoContact contact, HSSupplier supplier,
            HSAddressSupplier address, User user, ZohoCurrency currency)
        {
            return new ZohoContact()
            {
                contact_id = contact.contact_id,
                company_name = supplier.ID,
                contact_name = supplier.Name,
                contact_type = "vendor",
                billing_address = ZohoAddressMapper.Map(address),
                shipping_address = ZohoAddressMapper.Map(address),
                contact_persons = contact.contact_persons = (contact.contact_persons != null &&
                                                             contact.contact_persons.Any(c => c.email == user.Email))
                    ? new List<ZohoContactPerson>()
                    {
                        new ZohoContactPerson()
                        {
                            email = user.Email,
                            first_name = user.FirstName,
                            last_name = user.LastName,
                            phone = user.Phone
                        }
                    }
                    : null,
                currency_id = currency.currency_id
            };
        }

        public static ZohoContact Map(HSBuyer buyer, IList<HSUser> users, ZohoCurrency currency,
            HSBuyerLocation location)
        {
            return new ZohoContact()
            {
                company_name = $"{buyer.Name} - {location.Address?.xp.LocationID}",
                contact_name = $"{location.Address?.AddressName} - {location.Address?.xp.LocationID}",
                contact_type = "customer",
                billing_address = ZohoAddressMapper.Map(location.Address),
                shipping_address = ZohoAddressMapper.Map(location.Address),
                contact_persons = ZohoContactMapper.Map(users),
                currency_id = currency.currency_id,
                notes = $"Franchise ID: {buyer.ID} ~ Location ID: {location.Address?.xp.LocationID}"
            };
        }

        public static ZohoContact Map(ZohoContact contact, HSBuyer buyer, IList<HSUser> users,
            ZohoCurrency currency, HSBuyerLocation location)
        {
            contact.company_name = $"{buyer.Name} - {location.Address?.xp.LocationID}";
            contact.contact_name = $"{location.Address?.AddressName} - {location.Address?.xp.LocationID}";
            contact.contact_type = "customer";
            contact.billing_address = ZohoAddressMapper.Map(location.Address);
            contact.shipping_address = ZohoAddressMapper.Map(location.Address);
            contact.contact_persons = ZohoContactMapper.Map(users, contact);
            contact.currency_id = currency.currency_id;
            contact.notes = $"Franchise ID: {buyer.ID} ~ Location ID: {location.Address?.xp.LocationID}";
            return contact;
        }

        public static List<ZohoContactPerson> Map(IList<HSUser> users, ZohoContact contact = null)
        {
            // there is no property at this time for primary contact in OC, so we'll go with the first in the list
            var list = new List<ZohoContactPerson>();
            foreach (var user in users)
            {
                if (contact?.contact_persons != null && contact.contact_persons.Any(p => p.email == user.Email))
                {
                    var c = contact.contact_persons.FirstOrDefault(p => p.email == user.Email);
                    c.contact_person_id = c.contact_person_id;
                    c.email = user.Email;
                    c.first_name = user.FirstName;
                    c.last_name = user.LastName;
                    c.phone = user.Phone;
                    list.Add(c);
                }
                else
                {
                    list.Add(new ZohoContactPerson()
                    {
                        email = user.Email,
                        first_name = user.FirstName,
                        last_name = user.LastName,
                        phone = user.Phone,
                    });
                }
            }

            return list.DistinctBy(u => u.email).ToList();
        }
    }

    public static class ZohoShippingLineItemMapper
    {
        public static ZohoLineItem Map(ZohoLineItem item, HSShipMethod method)
        {
            item.item_id = item.item_id;
            item.item_type = "sales_and_purchases";
            item.name = $"Shipping: {method.Name}";
            item.rate = Math.Round(decimal.ToDouble(method.Cost), 2);
            item.description = $"{method.Name} - {method.EstimatedTransitDays} days transit";
            item.sku = item.sku;
            item.quantity = 1;
            item.unit = "each";
            item.purchase_description = $"{method.Name} - {method.EstimatedTransitDays} days transit";
            item.avatax_tax_code = "FR";
            return item;
        }

        public static ZohoLineItem Map(HSShipMethod method)
        {
            var item = new ZohoLineItem()
            {
                item_type = "sales_and_purchases",
                sku = method.ShippingSku(),
                rate = Math.Round(decimal.ToDouble(method.Cost), 2),
                description = $"{method.Name} - {method.EstimatedTransitDays} days transit",
                name = $"Shipping: {method.Name}",
                quantity = 1,
                unit = "each",
                purchase_description = $"{method.Name} - {method.EstimatedTransitDays} days transit",
                avatax_tax_code = "FR"
            };
            return item;
        }
    }

    public static class ZohoPurchaseLineItemMapper
    {
        public static ZohoLineItem Map(HSLineItem item, Supplier supplier)
        {
            return new ZohoLineItem()
            {
                purchase_description = $"{item.Product.Name ?? item.Variant?.Name} {item.Variant?.xp?.SpecCombo ?? item.SKU()}".Trim(),
                purchase_rate = item.UnitPrice.HasValue ? Math.Round(decimal.ToDouble(item.UnitPrice.Value), 2) : 0,
                manufacturer = supplier.Name,
            };
        }

        public static ZohoLineItem Map(ZohoLineItem zItem, HSLineItem item, Supplier supplier)
        {
            zItem.purchase_description = $"{item.Product.Name ?? item.Variant?.Name} {item.Variant?.xp?.SpecCombo ?? item.SKU()}".Trim();
            zItem.purchase_rate = item.UnitPrice.HasValue ? Math.Round(decimal.ToDouble(item.UnitPrice.Value), 2) : 0;
            zItem.manufacturer = supplier.Name;
            return zItem;
        }
    }

    public static class ZohoExtensions
    {
        public static string ShippingSuffix = "Shipping (41000)";
        public static string SKU(this HSLineItem item)
        {
            return item.Product == null ? "" : $"{item.Product.ID}-{item.Variant?.ID}".TrimEnd("-");
        }

        public static string ShippingSku(this HSShipMethod method)
        {
            return $"{method?.Name} {ShippingSuffix}";
        }
    }

    public static class ZohoSalesLineItemMapper
    {
        public static ZohoLineItem Map(HSLineItem item)
        {
            return new ZohoLineItem()
            {
                item_type = "sales_and_purchases",
                name = $"{item.Variant?.Name ?? item.Product.Name} {item.Variant?.xp?.SpecCombo ?? item.SKU()}".Trim(),
                rate = item.UnitPrice.HasValue ? Math.Round(decimal.ToDouble(item.UnitPrice.Value), 2) : 0,
                quantity = 1,
                description = $"{item.Variant?.Name ?? item.Product.Name} {item.Variant?.xp?.SpecCombo ?? item.SKU()}".Trim(),
                sku = item.SKU(),
                unit = item.Product.xp?.UnitOfMeasure?.Unit,
                avatax_tax_code = item.Product.xp?.Tax.Code
            };
        }

        public static ZohoLineItem Map(ZohoLineItem zItem, HSLineItem item)
        {
            zItem.item_type = "sales_and_purchases";
            zItem.name = $"{item.Variant?.Name ?? item.Product.Name} {item.Variant?.xp?.SpecCombo ?? item.SKU()}".Trim();
            zItem.description = $"{item.Variant?.Name ?? item.Product.Name} {item.Variant?.xp?.SpecCombo ?? item.SKU()}".Trim();
            zItem.purchase_description = $"{item.Variant?.Name ?? item.Product.Name} {item.Variant?.xp?.SpecCombo ?? item.SKU()}".Trim();
            zItem.rate = item.UnitPrice.HasValue ? Math.Round(decimal.ToDouble(item.UnitPrice.Value), 2) : 0;
            zItem.unit = item.Product.xp?.UnitOfMeasure?.Unit;
            zItem.avatax_tax_code = item.Product.xp?.Tax.Code;
            return zItem;
        }
    }

    public static class ZohoPurchaseOrderMapper
    {
        public static ZohoPurchaseOrder Map(ZohoSalesOrder salesorder, Order order, List<ZohoLineItem> items,
            List<HSLineItem> lineitems, ZohoAddress delivery_address, ZohoContact vendor, ZohoPurchaseOrder po)
        {
            po.line_items = items.Select(p => new ZohoLineItem()
            {
                //account_id = p.purchase_account_id,
                item_id = p.item_id,
                description = p.description,
                rate = Math.Round(decimal.ToDouble(lineitems.First(l => l.SKU() == p.sku).UnitPrice.Value), 2),
                quantity = lineitems.FirstOrDefault(li => li.SKU() == p.sku)?.Quantity
            }).ToList(); ;
            po.salesorder_id = salesorder.salesorder_id;
            po.purchaseorder_number = order.ID;
            po.reference_number = salesorder.reference_number;
            po.sub_total = decimal.ToDouble(order.Subtotal);
            po.tax_total = decimal.ToDouble(order.TaxCost);
            po.total = decimal.ToDouble(order.Total);
            po.vendor_id = vendor.contact_id;
            po.delivery_customer_id = salesorder.customer_id;
            return po;
        }

        public static ZohoPurchaseOrder Map(ZohoSalesOrder salesorder, Order order, List<ZohoLineItem> items,
            List<HSLineItem> lineitems, ZohoAddress delivery_address, ZohoContact vendor)
        {
            var po = new ZohoPurchaseOrder()
            {
                line_items = items.Select(p => new ZohoLineItem()
                {
                    //account_id = p.purchase_account_id,
                    item_id = p.item_id,
                    description = p.description,
                    rate = Math.Round(decimal.ToDouble(lineitems.First(l => l.SKU() == p.sku).UnitPrice.Value), 2),
                    quantity = lineitems.First(l => l.SKU() == p.sku)?.Quantity
                }).ToList(),
                salesorder_id = salesorder.salesorder_id,
                purchaseorder_number = order.ID,
                reference_number = salesorder.reference_number,
                sub_total = decimal.ToDouble(order.Subtotal),
                tax_total = decimal.ToDouble(order.TaxCost),
                total = decimal.ToDouble(order.Total),
                vendor_id = vendor.contact_id,
                delivery_customer_id = salesorder.customer_id
            };

            return po;
        }
    }

    public static class ZohoSalesOrderMapper
    {
        public static ZohoSalesOrder Map(ZohoSalesOrder zOrder, HSOrderWorksheet worksheet, List<ZohoLineItem> items, ZohoContact contact, IList<OrderPromotion> promotions)
        {
            zOrder.reference_number = worksheet.Order.ID;
            zOrder.salesorder_number = worksheet.Order.ID;
            zOrder.date = worksheet.Order.DateSubmitted?.ToString("yyyy-MM-dd");
            zOrder.is_discount_before_tax = true;
            zOrder.discount = decimal.ToDouble(promotions.Sum(p => p.Amount));
            zOrder.discount_type = "entity_level";
            zOrder.line_items = worksheet.LineItems.Select(item => new ZohoLineItem
            {
                item_id = items.First(i => i.sku == item.SKU()).item_id,
                quantity = item.Quantity,
                rate = Math.Round((double)(item.UnitPrice ?? 0), 2),
                avatax_tax_code = item.Product.xp.Tax.Code
                //discount = decimal.ToDouble(promotions.Where(p => p.LineItemLevel == true && p.LineItemID == line_item.ID).Sum(p => p.Amount)),
            }).ToList();
            zOrder.tax_total = decimal.ToDouble(worksheet.Order.TaxCost);
            zOrder.customer_name = contact.contact_name;
            zOrder.sub_total = decimal.ToDouble(worksheet.Order.Subtotal);
            zOrder.total = decimal.ToDouble(worksheet.Order.Total);
            zOrder.customer_id = contact.contact_id;
            zOrder.currency_code = contact.currency_code;
            zOrder.currency_symbol = contact.currency_symbol;
            zOrder.notes = promotions.Any()
                ? $"Promotions applied: {promotions.DistinctBy(p => p.Code).Select(p => p.Code).JoinString(" - ", p => p)}"
                : null;
            // adding shipping as a line item
            foreach (var shipment in worksheet.ShipEstimateResponse.ShipEstimates)
            {
                var method = shipment.ShipMethods.FirstOrDefault(s => s.ID == shipment.SelectedShipMethodID);
                zOrder.line_items.Add(new ZohoLineItem
                {
                    item_id = items.First(i => i.sku == method?.ShippingSku()).item_id,
                    quantity = 1,
                    rate = Math.Round((double)(method?.Cost ?? 0), 2),
                    avatax_tax_code = "FR"
                });
            }
            return zOrder;
        }

        public static ZohoSalesOrder Map(HSOrderWorksheet worksheet, List<ZohoLineItem> items, ZohoContact contact, IList<OrderPromotion> promotions)
        {
            var o = new ZohoSalesOrder()
            {
                reference_number = worksheet.Order.ID,
                salesorder_number = worksheet.Order.ID,
                date = worksheet.Order.DateSubmitted?.ToString("yyyy-MM-dd"),
                is_discount_before_tax = true,
                discount = decimal.ToDouble(promotions.Sum(p => p.Amount)),
                discount_type = "entity_level",
                line_items = worksheet.LineItems.Select(item => new ZohoLineItem
                {
                    item_id = items.First(i => i.sku == item.SKU()).item_id,
                    quantity = item.Quantity,
                    rate = Math.Round((double)(item.UnitPrice ?? 0), 2),
                    avatax_tax_code = item.Product.xp.Tax.Code
                    //discount = decimal.ToDouble(promotions.Where(p => p.LineItemLevel == true && p.LineItemID == line_item.ID).Sum(p => p.Amount)),
                }).ToList(),
                tax_total = decimal.ToDouble(worksheet.Order.TaxCost),
                customer_name = contact.contact_name,
                sub_total = decimal.ToDouble(worksheet.Order.Subtotal),
                total = decimal.ToDouble(worksheet.Order.Total),
                customer_id = contact.contact_id,
                currency_code = contact.currency_code,
                currency_symbol = contact.currency_symbol,
                notes = promotions.Any()
                    ? $"Promotions applied: {promotions.DistinctBy(p => p.Code).Select(p => p.Code).JoinString(" - ", p => p)}"
                    : null
                //shipping_charge = decimal.ToDouble(order.ShippingCost), //TODO: Please mention any Shipping/miscellaneous charges as additional line items.
            };
            // adding shipping as a line item
            foreach (var shipment in worksheet.ShipEstimateResponse.ShipEstimates)
            {
                var method = shipment.ShipMethods.FirstOrDefault(s => s.ID == shipment.SelectedShipMethodID);
                o.line_items.Add(new ZohoLineItem
                {
                    item_id = items.First(i => i.sku == method?.ShippingSku()).item_id,
                    quantity = 1,
                    rate = Math.Round((double)(method?.Cost ?? 0), 2),
                    avatax_tax_code = "FR"
                });
            }
            return o;
        }
    }

    public static class ZohoAddressMapper
    {
        public static ZohoAddress Map(HSAddressSupplier address)
        {
            return new ZohoAddress()
            {
                attention = address.CompanyName,
                address = address.Street1,
                street2 = address.Street2,
                city = address.City,
                state = address.State,
                zip = address.Zip,
                country = address.Country,
                phone = address.Phone,
                state_code = address.State
            };
        }

        public static ZohoAddress Map(HSAddressBuyer address)
        {
            return new ZohoAddress()
            {
                attention = address.CompanyName,
                address = address.Street1,
                street2 = address.Street2,
                city = address.City,
                state = address.State,
                zip = address.Zip,
                country = address.Country,
                phone = address.Phone,
                state_code = address.State
            };
        }
    }
}