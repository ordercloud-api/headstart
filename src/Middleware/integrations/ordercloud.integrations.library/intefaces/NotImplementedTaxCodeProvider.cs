using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ordercloud.integrations.library.intefaces
{
	public class NotImplementedTaxCodesProvider: ITaxCodesProvider
	{
		public Task<TaxCategorizationResponse> ListTaxCodesAsync(string searchTerm)
		{
			return Task.FromResult(new TaxCategorizationResponse() 
			{
				ProductsShouldHaveTaxCodes = false, 
				Categories = new List<TaxCategorization>() 
			});
		}
	}
}
