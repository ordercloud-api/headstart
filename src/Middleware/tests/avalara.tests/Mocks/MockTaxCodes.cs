using Avalara.AvaTax.RestClient;
using ordercloud.integrations.avalara;
using System;
using System.Collections.Generic;
using System.Text;

namespace avalara.tests.Mocks
{
	class MockTaxCodes
	{
		public static FetchResult<TaxCodeModel> taxCodeObjectFromAvalaraFirstRecord()
		{
			return new FetchResult<TaxCodeModel>()
			{
				count = 1,
				value = new List<TaxCodeModel>()
				{
					new TaxCodeModel
					{
						id = 9934,
						companyId = 1,
						taxCode = "Test-Tax-Code",
						taxCodeTypeId = "P",
						description = "Test Tax Code Description",
						parentTaxCode = "PP030100",
						isPhysical = true,
						goodsServiceCode = 0,
						isActive = true,
						isSSTCertified = true,
						createdDate = DateTime.Parse("2006-01-24T04:59:48.27"),
						createdUserId = 0,
						modifiedDate = DateTime.Parse("2013-03-27T22:54:25.363"),
						modifiedUserId = 0
					}
				}
			};
		}
		public static FetchResult<TaxCodeModel> taxCodeObjectFromAvalaraSecondRecord()
		{
			return new FetchResult<TaxCodeModel>()
			{
				count = 1,
				value = new List<TaxCodeModel>()
				{
					new TaxCodeModel
					{
						id = 9934,
						companyId = 1,
						taxCode = "Second-Test-Tax-Code",
						taxCodeTypeId = "P",
						description = "Second Test Tax Code Description",
						parentTaxCode = "PP030100",
						isPhysical = true,
						goodsServiceCode = 0,
						isActive = true,
						isSSTCertified = true,
						createdDate = DateTime.Parse("2006-01-24T04:59:48.27"),
						createdUserId = 0,
						modifiedDate = DateTime.Parse("2013-03-27T22:54:25.363"),
						modifiedUserId = 0
					}
				}
			};
		}
		public static FetchResult<TaxCodeModel> taxCodeObjectFromAvalaraAllRecords()
		{
			return new FetchResult<TaxCodeModel>()
			{
				count = 2,
				value = new List<TaxCodeModel>()
				{
					new TaxCodeModel
					{
						id = 9934,
						companyId = 1,
						taxCode = "Test-Tax-Code",
						taxCodeTypeId = "P",
						description = "Test Tax Code Description",
						parentTaxCode = "PP030100",
						isPhysical = true,
						goodsServiceCode = 0,
						isActive = true,
						isSSTCertified = true,
						createdDate = DateTime.Parse("2006-01-24T04:59:48.27"),
						createdUserId = 0,
						modifiedDate = DateTime.Parse("2013-03-27T22:54:25.363"),
						modifiedUserId = 0
					},
					new TaxCodeModel
					{
						id = 9934,
						companyId = 1,
						taxCode = "Second-Test-Tax-Code",
						taxCodeTypeId = "P",
						description = "Second Test Tax Code Description",
						parentTaxCode = "PP030100",
						isPhysical = true,
						goodsServiceCode = 0,
						isActive = true,
						isSSTCertified = true,
						createdDate = DateTime.Parse("2006-01-24T04:59:48.27"),
						createdUserId = 0,
						modifiedDate = DateTime.Parse("2013-03-27T22:54:25.363"),
						modifiedUserId = 0
					}
				}
			};
		}
		public static List<TaxCode> headstartTaxCodeListPageFirstRecord()
		{
			return new List<TaxCode>

			{
				new TaxCode()
				{
					Category = "Test-Category",
					Code = "Test-Tax-Code",
					Description = "Test Tax Code Description"
				}
			};
		}
		public static List<TaxCode> headstartTaxCodeListPageSecondRecord()
		{
			return new List<TaxCode>

			{
				new TaxCode()
				{
					Category = "Second-Test-Category",
					Code = "Second-Test-Tax-Code",
					Description = "Second Test Tax Code Description"
				}
			};
		}
		public static List<TaxCode> headstartTaxCodeListPageAllRecords()
		{
			return new List<TaxCode>

			{
				new TaxCode()
				{
					Category = "Test-Category",
					Code = "Test-Tax-Code",
					Description = "Test Tax Code Description"
				},
				new TaxCode()
				{
					Category = "Second-Test-Category",
					Code = "Second-Test-Tax-Code",
					Description = "Second Test Tax Code Description"
				}
			};
		}
	}
}
