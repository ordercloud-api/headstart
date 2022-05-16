﻿using System.Collections.Generic;
using System.Linq;
using OrderCloud.Integrations.Library.Interfaces;
using Taxjar;

namespace OrderCloud.Integrations.TaxJar.Mappers
{
    public static class TaxJarCategoryMapper
    {
        public static TaxCategorizationResponse ToTaxCategorization(this List<Category> categories, string searchTerm)
        {
            IEnumerable<Category> toReturn;
            if (searchTerm == null || searchTerm == string.Empty)
            {
                toReturn = categories;
            }
            else
            {
                toReturn = categories.Where(c =>
                {
                    var search = searchTerm.ToLower();
                    return c.Name.ToLower().Contains(search) || c.Description.ToLower().Contains(search);
                });
            }

            var list = toReturn.Select(c => new TaxCategorization()
            {
                Code = c.ProductTaxCode,
                Description = c.Name,
                LongDescription = c.Description,
            }).ToList();
            return new TaxCategorizationResponse() { Categories = list, ProductsShouldHaveTaxCodes = true };
        }
    }
}
