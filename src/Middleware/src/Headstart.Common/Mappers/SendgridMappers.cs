using Headstart.Models;
using Headstart.Models.Extended;
using Headstart.Models.Headstart;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static Headstart.Common.Models.SendGridModels;

namespace Headstart.Common.Mappers
{
    public static class SendgridMappers
    {
        public static QuoteOrderTemplateData GetQuoteOrderTemplateData(HSOrder order, IList<HSLineItem> lineItems)
        {
            return new QuoteOrderTemplateData()
            {
                FirstName = order.xp?.QuoteOrderInfo?.FirstName,
                LastName = order.xp?.QuoteOrderInfo?.LastName,
                Phone = order.xp?.QuoteOrderInfo?.Phone,
                Email = order.xp?.QuoteOrderInfo?.Email,
                Location = order.xp?.QuoteOrderInfo?.BuyerLocation,
                ProductID = lineItems.FirstOrDefault().Product.ID,
                ProductName = lineItems.FirstOrDefault().Product.Name,
                Order = order,
            };
        }

        public static List<string> GetSupplierInfo(ListPage<HSLineItem> lineItems)
        {
            var supplierList = lineItems?.Items?.Select(item => item.SupplierID)
                .Distinct()
                .ToList();
            return supplierList;
        }

        public static OrderTemplateData GetOrderTemplateData(HSOrder order, IList<HSLineItem> lineItems)
        {
            var productsList = lineItems?.Select(lineItem =>
            {
                return new LineItemProductData()
                {
                    ProductName = lineItem?.Product?.Name,
                    ImageURL = lineItem?.xp?.ImageUrl,
                    ProductID = lineItem?.ProductID,
                    Quantity = lineItem?.Quantity,
                    LineTotal = lineItem?.LineTotal,
                    SpecCombo = GetSpecCombo(lineItem?.Specs)
                };
            });
            var shippingAddress = GetShippingAddress(lineItems);
            var currencyString = order.xp?.Currency?.ToString();
            return new OrderTemplateData()
            {
                FirstName = order?.FromUser?.FirstName,
                LastName = order?.FromUser?.LastName,
                OrderID = order?.ID,
                DateSubmitted = order?.DateSubmitted?.ToString(),
                ShippingAddressID = order?.ShippingAddressID,
                ShippingAddress = shippingAddress,
                BillingAddressID = order?.BillingAddressID,
                BillingAddress = new Address()
                {
                    Street1 = order?.BillingAddress?.Street1,
                    Street2 = order?.BillingAddress?.Street2,
                    City = order?.BillingAddress?.City,
                    State = order?.BillingAddress?.State,
                    Zip = order?.BillingAddress?.Zip
                },
                BillTo = null,
                Products = productsList,
                Subtotal = order?.Subtotal,
                TaxCost = order?.TaxCost,
                ShippingCost = order?.ShippingCost,
                PromotionalDiscount = order?.PromotionDiscount,
                Total = order?.Total,
                Currency = currencyString,
                Comments = order.Comments
            };
        }

        public static string GetSpecCombo(IList<LineItemSpec> specs)
        {
            if (specs == null || !specs.Any())
            {
                return null;
            }
            string specCombo = "(" + string.Join(", ", specs.Select(spec => spec.Value).ToArray()) + ")";
            return specCombo;
        }

        public static string DetermineRecipient(AppSettings settings, string subject)
        {
            switch (subject.ToLower())
            {
                case "general":
                    return settings.SendgridSettings.SupportCaseEmail;
                case "report an error/bug":
                    return settings.SendgridSettings.SupportCaseEmail;
                case "payment, billing, or refunds":
                    return settings.SendgridSettings.BillingEmail;
                default:
                    return settings.SendgridSettings.SupportCaseEmail;
            }
        }

        public static List<object> MapLineItemsToProducts(ListPage<HSLineItem> lineItems, string actionType)
        {
            List<object> products = new List<object>();

            foreach (var lineItem in lineItems.Items)
            {
                if (lineItem.xp.Returns != null && actionType == "return")
                {
                    products.Add(MapReturnedLineItemToProduct(lineItem));
                }
                else if (lineItem.xp.Cancelations != null && actionType == "cancel")
                {
                    products.Add(MapCanceledLineItemToProduct(lineItem));
                }
                else
                {
                    products.Add(MapLineItemToProduct(lineItem));
                }
            }
            return products;
        }

        public static LineItemProductData MapReturnedLineItemToProduct(HSLineItem lineItem) =>
        lineItem == null ? null :
            new LineItemProductData()
            {
                ProductName = lineItem?.Product?.Name,
                ImageURL = lineItem?.xp?.ImageUrl,
                ProductID = lineItem?.ProductID,
                Quantity = lineItem?.Quantity,
                LineTotal = lineItem?.LineTotal,
            };

        public static LineItemProductData MapCanceledLineItemToProduct(HSLineItem lineItem) =>
        lineItem == null ? null :
            new LineItemProductData()
            {
                ProductName = lineItem?.Product?.Name,
                ImageURL = lineItem?.xp?.ImageUrl,
                ProductID = lineItem?.ProductID,
                Quantity = lineItem?.Quantity,
                LineTotal = lineItem?.LineTotal,
            };

        public static LineItemProductData MapLineItemToProduct(HSLineItem lineItem) =>
        lineItem == null ? null :
          new LineItemProductData()
          {
              ProductName = lineItem?.Product?.Name,
              ImageURL = lineItem?.xp?.ImageUrl,
              ProductID = lineItem?.ProductID,
              Quantity = lineItem?.Quantity,
              LineTotal = lineItem?.LineTotal,
          };

        public static Address GetShippingAddress(IList<HSLineItem> lineItems) =>
        lineItems == null ? null : 
            new Address()
            {
                Street1 = lineItems[0]?.ShippingAddress?.Street1,
                Street2 = lineItems[0]?.ShippingAddress?.Street2,
                City = lineItems[0]?.ShippingAddress?.City,
                State = lineItems[0]?.ShippingAddress?.State,
                Zip = lineItems[0]?.ShippingAddress?.Zip
            };
  
        public static LineItemProductData MapToTemplateProduct(HSLineItem lineItem, LineItemStatusChange lineItemStatusChange, LineItemStatus status)
        {
            decimal lineTotal = 0M;
            if (status == LineItemStatus.ReturnDenied || status == LineItemStatus.CancelDenied && lineItemStatusChange.QuantityRequestedForRefund != lineItemStatusChange.Quantity)
            {
                int quantityApproved = lineItemStatusChange.QuantityRequestedForRefund - lineItemStatusChange.Quantity;
                decimal costPerUnitAfterTaxes = (decimal)(lineItemStatusChange.Refund / quantityApproved);
                lineTotal = Math.Round(costPerUnitAfterTaxes * lineItemStatusChange.Quantity, 2);
            }
            else
            {
                lineTotal = lineItemStatusChange.Refund ?? lineItem.LineTotal;
            }
            return new LineItemProductData
            {
                ProductName = lineItem?.Product?.Name,
                ImageURL = lineItem?.xp?.ImageUrl,
                ProductID = lineItem?.ProductID,
                Quantity = lineItem?.Quantity,
                LineTotal = lineTotal,
                QuantityChanged = lineItemStatusChange?.Quantity,
                MessageToBuyer = lineItemStatusChange.Comment
            };
        }


    }
}
