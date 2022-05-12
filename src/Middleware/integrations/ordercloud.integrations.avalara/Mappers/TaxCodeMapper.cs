using Avalara.AvaTax.RestClient;
using OrderCloud.SDK;
using System;
using System.Linq;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using ordercloud.integrations.library.intefaces;
using System.Collections.Generic;
using TaxCategorization = ordercloud.integrations.library.intefaces.TaxCategorization;

namespace ordercloud.integrations.avalara
{
	public static class TaxCodeMapper
	{
		// Tax Codes for lines on Transactions
		public static List<TaxCategorization> MapTaxCodes(FetchResult<TaxCodeModel> codes)
		{
			return codes.value.Select(code => new TaxCategorization
			{
				Code = code.taxCode,
				Description = code.description
			}).ToList();
		}

		public static string MapSearchString(string searchTerm)
		{
			var searchString = $"isActive eq true";
			if (searchTerm != string.Empty)
			{
				searchString = $"{searchString} and (taxCode contains '{searchTerm}' OR description contains '{searchTerm}')";
			}

			return searchString;
		}
	}
}
