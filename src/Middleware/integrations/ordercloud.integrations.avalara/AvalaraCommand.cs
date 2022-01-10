using Avalara.AvaTax.RestClient;
using Flurl;
using Flurl.Http;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using System.Linq;
using OrderCloud.Catalyst;
using ordercloud.integrations.library.intefaces;

namespace ordercloud.integrations.avalara
{
	public interface IAvalaraCommand
	{
		/// <summary>
		/// Calculates tax for an order without creating any records. Use this to display tax amount to user prior to order submit.
		/// </summary>
		Task<OrderTaxCalculation> CalculateEstimateAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions);
		/// <summary>
		/// Creates a tax transaction record in the calculating system. Use this once on purchase, payment capture, or fulfillment.
		/// </summary>
		Task<OrderTaxCalculation> CommitTransactionAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions);
		Task<TaxCategorizationResponse> ListTaxCodesAsync(string searchTerm);
	}

	public enum AppEnvironment { Test, Staging, Production }

	public class AvalaraCommand : IAvalaraCommand, ITaxCalculator, ITaxCodesProvider
	{
		private readonly AvalaraConfig _settings;
		private readonly AvaTaxClient _avaTax;
		private readonly string _companyCode;
		private readonly string _baseUrl;
		private bool hasAccountCredentials;
		private AppEnvironment appEnvironment;

		public AvalaraCommand(AvalaraConfig settings,  string environment)
		{
			_settings = settings;
			appEnvironment = (AppEnvironment)Enum.Parse(typeof(AppEnvironment), environment);

			hasAccountCredentials = !string.IsNullOrEmpty(_settings?.LicenseKey);

			_companyCode = _settings.CompanyCode;
			_baseUrl = _settings.BaseApiUrl;
			if(hasAccountCredentials) {
				_avaTax = new AvaTaxClient("four51_headstart", "v1", "four51_headstart", new Uri(settings.BaseApiUrl)).WithSecurity(settings.AccountID, settings.LicenseKey);
			}
        }

		private bool ShouldMockAvalaraResponse()
        {
			// To give a larger "headstart" in Test and UAT, Responses can be mocked by simply
			// not providing an Avalara License Key. (It is still needed for Production)
			return !hasAccountCredentials && appEnvironment != AppEnvironment.Production;
		}

		public async Task<OrderTaxCalculation> CalculateEstimateAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions)
		{
			if (ShouldMockAvalaraResponse()) { return CreateMockTransactionModel(); }
			var taxEstimate = await CreateTransactionAsync(DocumentType.SalesOrder, orderWorksheet, promotions);
			return taxEstimate;
		}

        private OrderTaxCalculation CreateMockTransactionModel()
        {
			TransactionModel result = new TransactionModel()
			{
				totalTax = (decimal?)123.45,
				code = "Mock Avalara Response for Headstart",
				date = DateTime.Now
			};

			 return result.ToOrderTaxCalculation();
		}

        private TaxCategorizationResponse CreateMockTaxCategorizationResponseModel()
        {
            return new TaxCategorizationResponse()
            {
                ProductsShouldHaveTaxCodes = true,
                Categories = new List<TaxCategorization>()
                {
                    new TaxCategorization()
                    {
                        Code = "Headstart Tax Code",
                        Description = "Mock Tax Code for Headstart",
                        LongDescription = "This is a mock tax categorization"
                    }
                }
            };
        }

        public async Task<OrderTaxCalculation> CommitTransactionAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions)
		{
			if (ShouldMockAvalaraResponse()) { return CreateMockTransactionModel(); }

			var transaction = await CreateTransactionAsync(DocumentType.SalesInvoice, orderWorksheet, promotions);
			return transaction;
		}

		public async Task<OrderTaxCalculation> CommitTransactionAsync(string transactionCode)
		{
			if (ShouldMockAvalaraResponse()) { return CreateMockTransactionModel(); }

			var model = new CommitTransactionModel() { commit = true };
			var transaction = await _avaTax.CommitTransactionAsync(_companyCode, transactionCode, DocumentType.SalesInvoice, "", model);
			return transaction.ToOrderTaxCalculation();
		}

		public async Task<TaxCategorizationResponse> ListTaxCodesAsync(string searchTerm)
		{
            if (ShouldMockAvalaraResponse()) { return CreateMockTaxCategorizationResponseModel(); }

            var search = TaxCodeMapper.MapSearchString(searchTerm);
			var avataxCodes = await _avaTax.ListTaxCodesAsync(search, null, null, null);
			var codeList = TaxCodeMapper.MapTaxCodes(avataxCodes);
			return new TaxCategorizationResponse() { Categories = codeList, ProductsShouldHaveTaxCodes = true };
		}

		private async Task<OrderTaxCalculation> CreateTransactionAsync(DocumentType docType, OrderWorksheet orderWorksheet, List<OrderPromotion> promotions)
		{
			var standardLineItems = orderWorksheet.LineItems.Where(li => li.Product.xp.ProductType == "Standard")?.ToList();
            if (standardLineItems.Any())
            {
				try
				{
					if (ShouldMockAvalaraResponse()) { return CreateMockTransactionModel(); }
					var createTransactionModel = orderWorksheet.ToAvalaraTransactionModel(_companyCode, docType, promotions);
					var transaction = await _avaTax.CreateTransactionAsync("", createTransactionModel);
					return transaction.ToOrderTaxCalculation();
				}
				catch (AvaTaxError e)
				{
					throw new CatalystBaseException("AvalaraTaxError", e.error.error.message, e.error.error, 400);
				}
			} else
            {
				return new OrderTaxCalculation
				{
					OrderID = orderWorksheet.Order.ID,
					ExternalTransactionID = "NotTaxable",
					TotalTax = 0
				};
            }
			
		}
	}
}
