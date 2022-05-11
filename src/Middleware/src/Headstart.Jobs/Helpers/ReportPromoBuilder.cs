using System.Collections.Generic;
using System.Linq;
using Headstart.Common.Models;
using OrderCloud.SDK;
using Headstart.Models.Headstart;

namespace Headstart.Jobs.Helpers
{
    public static class ReportPromoBuilder
    {
        public static PromotionData BuildPromoFields(ListPage<OrderPromotion> promotions, ReportTypeEnum reportType, string supplierID = null, List<HSLineItem> discountedLineItems = null)
        {
            var uniquePromotions = promotions.Items.GroupBy(promotion => promotion.Code).Select(promo => promo.First());

            if (reportType == ReportTypeEnum.PurchaseOrderDetail)
            {
                var supplierRelatedPromotions = new List<OrderPromotion>();

                foreach (var promo in uniquePromotions)
                {
                    if (!promo.LineItemLevel)
                    {
                        supplierRelatedPromotions.Add(promo);
                        continue;
                    }

                    if (promo.xp?.Supplier == supplierID)
                    {
                        supplierRelatedPromotions.Add(promo);
                        continue;
                    }

                    foreach (var sku in promo.xp?.SKUs)
                    {
                        if (discountedLineItems.Any(line => line.ProductID == sku && line.SupplierID == supplierID))
                        {
                            supplierRelatedPromotions.Add(promo);
                        }
                    }
                }

                uniquePromotions = supplierRelatedPromotions;
            }

            var reportFormattedPromoData = new PromotionData();
            foreach (var promo in uniquePromotions)
            {
                var delimiter = "";
                if (reportFormattedPromoData.PromoCode != null)
                {
                    delimiter = " | ";
                }
                // Promotions always have values for these fields
                reportFormattedPromoData.PromoCode += delimiter + promo.Code;
                reportFormattedPromoData.SupplierSpecific += delimiter + promo.xp?.ScopeToSupplier.ToString().ToLower();
                reportFormattedPromoData.OrderLevelPromo += delimiter + (!promo.LineItemLevel).ToString().ToLower();
                reportFormattedPromoData.LineItemLevelPromo += delimiter + promo.LineItemLevel.ToString().ToLower();

                if (reportFormattedPromoData.PromoSupplierName == null)
                {
                    delimiter = "";
                }
                // Field could be null
                reportFormattedPromoData.PromoSupplierName += delimiter + promo.xp?.Supplier;
            }

            return reportFormattedPromoData;
        }
    }
}
