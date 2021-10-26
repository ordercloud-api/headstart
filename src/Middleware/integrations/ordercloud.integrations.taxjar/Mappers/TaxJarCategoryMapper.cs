using ordercloud.integrations.library.intefaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taxjar;

namespace ordercloud.integrations.taxjar
{
	public static class TaxJarCategoryMapper
	{
		public static TaxCategorizationResponse ToTaxCategorization(this List<Category> categories, string searchTerm)
		{
			IEnumerable<Category> toReturn;
			if (searchTerm == null || searchTerm == "")
			{
				toReturn = categories;
			} else
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
				LongDescription = c.Description
			}).ToList();
			return new TaxCategorizationResponse() { Categories = list, ProductsShouldHaveTaxCodes = true };
		}
	}
}
