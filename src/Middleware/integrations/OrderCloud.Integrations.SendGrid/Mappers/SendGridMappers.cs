using System;
using System.Collections.Generic;
using System.Linq;
using Headstart.Common.Models;
using OrderCloud.SDK;
using static OrderCloud.Integrations.SendGrid.SendGridModels;

namespace OrderCloud.Integrations.SendGrid
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
                UnitPrice = lineItems.FirstOrDefault().UnitPrice,
                Currency = order.xp?.Currency.ToString(),
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
                    SpecCombo = GetSpecCombo(lineItem?.Specs),
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
                    Zip = order?.BillingAddress?.Zip,
                },
                BillTo = null,
                Products = productsList,
                Subtotal = order?.Subtotal,
                TaxCost = order?.TaxCost,
                ShippingCost = order?.ShippingCost,
                PromotionalDiscount = order?.PromotionDiscount,
                Total = order?.Total,
                Currency = currencyString,
                Comments = order.Comments,
            };
        }

        public static OrderReturnTemplateData GetOrderReturnTemplateData(HSOrder order, IList<HSLineItem> lineItems, HSOrderReturn orderReturn, IList<OrderApproval> orderApprovals)
        {
            var orderData = GetOrderTemplateData(order, lineItems);
            return new OrderReturnTemplateData
            {
                // order data
                FirstName = orderData.FirstName,
                LastName = orderData.LastName,
                OrderID = orderData.OrderID,
                DateSubmitted = orderData.DateSubmitted,
                ShippingAddressID = orderData.ShippingAddressID,
                ShippingAddress = orderData.ShippingAddress,
                BillingAddressID = orderData.BillingAddressID,
                BillingAddress = orderData.BillingAddress,
                BillTo = orderData.BillTo,
                Products = orderData.Products,
                Subtotal = orderData.Subtotal,
                TaxCost = orderData.TaxCost,
                ShippingCost = orderData.ShippingCost,
                PromotionalDiscount = orderData.PromotionalDiscount,
                Total = orderData.Total,
                Currency = orderData.Currency,
                Comments = orderData.Comments,

                // order return data
                OrderReturnID = orderReturn.ID,
                OrderReturnRefundAmount = orderReturn.RefundAmount,
                OrderReturnBuyerComments = orderReturn.Comments,
                OrderReturnSellerComments = orderReturn.xp?.SellerComments,
                OrderReturnItemsToReturn = orderReturn.ItemsToReturn.Select(item => new EnhancedOrderReturnItem
                {
                    LineItemID = item.LineItemID,
                    LineItem = lineItems.FirstOrDefault(li => li.ID == item.LineItemID),
                    Quantity = item.Quantity,
                    RefundAmount = item.RefundAmount,
                    Comments = item.Comments,
                }),
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

        public static string DetermineRecipient(SendGridSettings sendgridSettings, string subject)
        {
            switch (subject.ToLower())
            {
                case "general":
                    return sendgridSettings.SupportCaseEmail;
                case "report an error/bug":
                    return sendgridSettings.SupportCaseEmail;
                case "payment, billing, or refunds":
                    return sendgridSettings.BillingEmail;
                default:
                    return sendgridSettings.SupportCaseEmail;
            }
        }

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
                Zip = lineItems[0]?.ShippingAddress?.Zip,
            };

        public static LineItemProductData MapToTemplateProduct(HSLineItem lineItem, LineItemStatusChange lineItemStatusChange, LineItemStatus status)
        {
            return new LineItemProductData
            {
                ProductName = lineItem?.Product?.Name,
                ImageURL = lineItem?.xp?.ImageUrl,
                ProductID = lineItem?.ProductID,
                Quantity = lineItem?.Quantity,
                LineTotal = lineItem.LineTotal,
                QuantityChanged = lineItemStatusChange?.Quantity,
            };
        }
    }
}
