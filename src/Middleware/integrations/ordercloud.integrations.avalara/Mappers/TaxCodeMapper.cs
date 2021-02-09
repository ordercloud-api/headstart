using Avalara.AvaTax.RestClient;
using OrderCloud.SDK;
using System;
using System.Linq;
using ordercloud.integrations.library;

namespace ordercloud.integrations.avalara
{
	public static class TaxCodeMapper
	{
		// Tax Codes for lines on Transactions

		public static ListPage<TaxCode> Map(FetchResult<TaxCodeModel> codes, TaxCodesListArgs args)
		{
			var items = codes.value.Select(code => new TaxCode
			{
				Category = args.CodeCategory,
				Code = code.taxCode,
				Description = code.description
			}).ToList();
			var listPage = new ListPage<TaxCode>
			{
				Items = items,
				Meta = new ListPageMeta
				{
					Page = (int)Math.Ceiling((double)args.Skip / args.Top) + 1,
					PageSize = 100,
					TotalCount = codes.count,
				}
			};
			return listPage;
		}

		public static TaxCodesListArgs Map(ListArgs<TaxCode> source)
		{
			var taxCategory = source?.Filters?[0]?.Values?[0]?.Term ?? ""; // TODO - error if no term provided
			var taxCategorySearch = taxCategory.Trim('0');
			var search = source.Search;
			var filter = search != "" ? $"isActive eq true and taxCode startsWith '{taxCategorySearch}' and (taxCode contains '{search}' OR description contains '{search}')" : $"isActive eq true and taxCode startsWith '{taxCategorySearch}'";
			return new TaxCodesListArgs()
			{
				Filter = filter,
				Top = source.PageSize,
				Skip = (source.Page - 1) * source.PageSize,
				CodeCategory = taxCategory,
				OrderBy = null
			};
		}
	}
}
