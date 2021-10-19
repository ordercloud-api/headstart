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
		Task<List<TaxCategorization>> ListTaxCodesAsync(string searchTerm);
		Task<TaxCertificate> GetCertificateAsync(int certificateID);
		Task<TaxCertificate> CreateCertificateAsync(TaxCertificate cert, Address buyerLocation);
		Task<TaxCertificate> UpdateCertificateAsync(int certificateID, TaxCertificate cert, Address buyerLocation);
	}

	public enum AppEnvironment { Test, Staging, Production }

	public class AvalaraCommand : IAvalaraCommand, ITaxCalculator, ITaxCodesProvider
	{
		private readonly AvalaraConfig _settings;
		private readonly AvaTaxClient _avaTax;
		private readonly string _companyCode;
		private readonly string _baseUrl;
		private bool noAccountCredentials;
		private AppEnvironment appEnvironment;

		public AvalaraCommand(AvalaraConfig settings, AvaTaxClient client, string environment)
		{
			_settings = settings;
			appEnvironment = (AppEnvironment)Enum.Parse(typeof(AppEnvironment), environment);

			noAccountCredentials = string.IsNullOrEmpty(_settings?.LicenseKey);

			_companyCode = _settings.CompanyCode;
			_baseUrl = _settings.BaseApiUrl;
            _avaTax = client;
        }

		private bool ShouldMockAvalaraResponse()
        {
			// To give a larger "headstart" in Test and UAT, Responses can be mocked by simply
			// not providing an Avalara License Key. (It is still needed for Production)
			return noAccountCredentials && appEnvironment != AppEnvironment.Production;
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

		public async Task<List<TaxCategorization>> ListTaxCodesAsync(string searchTerm)
		{
			if (ShouldMockAvalaraResponse()) { return CreateMockTaxCodeList(); }

			var search = TaxCodeMapper.MapSearchString(searchTerm);
			var avataxCodes = await _avaTax.ListTaxCodesAsync(search, null, null, null);
			var codeList = TaxCodeMapper.MapTaxCodes(avataxCodes);
			return codeList;
		}

        private List<TaxCategorization> CreateMockTaxCodeList()
        {
			return new List<TaxCategorization>() { 
				new TaxCategorization() {Description = "Mock Tax Code for Headstart", Code = "Headstart Tax Code" },
			};
        }

        public async Task<TaxCertificate> GetCertificateAsync(int certificateID)
		{
			if (ShouldMockAvalaraResponse()) { return CreateMockTaxCertificate(); }

			var companyID = _settings.CompanyID;
			var certificate = _avaTax.GetCertificateAsync(companyID, certificateID, "");
			var pdf = GetCertificateBase64String(companyID, certificateID);
			var mappedCertificate = TaxCertificateMapper.Map(await certificate, await pdf);
			return mappedCertificate;
		}

		public async Task<TaxCertificate> CreateCertificateAsync(TaxCertificate cert, Address buyerLocation)
		{
			if (ShouldMockAvalaraResponse()) { return CreateMockTaxCertificate(); }

			var companyID = _settings.CompanyID;
			var certificates = await _avaTax.CreateCertificatesAsync(companyID, false, new List<CertificateModel> { 
				TaxCertificateMapper.Map(cert, buyerLocation, companyID) 
			});
			var pdf = await GetCertificateBase64String(companyID, certificates[0].id ?? 0);
			var mappedCertificate = TaxCertificateMapper.Map(certificates[0], pdf);
			return mappedCertificate;
		}

        private TaxCertificate CreateMockTaxCertificate()
        {
			//bypass calling Avalara when no License Key is provided. 
			return new TaxCertificate()
			{
				FileName = "Mock Tax Certificate",
				SignedDate = DateTimeOffset.Now
			};
        }

        public async Task<TaxCertificate> UpdateCertificateAsync(int certificateID, TaxCertificate cert, Address buyerLocation)
		{
			if (ShouldMockAvalaraResponse()) { return CreateMockTaxCertificate(); }

			var companyID = _settings.CompanyID;
			var certificate = _avaTax.UpdateCertificateAsync(companyID, certificateID, TaxCertificateMapper.Map(cert, buyerLocation, companyID));
			var pdf = GetCertificateBase64String(companyID, certificateID);
			var mappedCertificate = TaxCertificateMapper.Map(await certificate, await pdf);
			return mappedCertificate;
		}

		// The avalara SDK method for this was throwing an internal JSON parse exception.
		private async Task<string> GetCertificateBase64String(int companyID, int certificateID)
		{
			//When no credentials are supplied, certificates can't be uploaded or downloaded, so return empty
			if (ShouldMockAvalaraResponse()) { return ""; }

			var pdfBtyes = await new Url($"{_baseUrl}/companies/{companyID}/certificates/{certificateID}/attachment")
				.WithBasicAuth(_settings.AccountID.ToString(), _settings.LicenseKey)
				.GetBytesAsync();
			return Convert.ToBase64String(pdfBtyes);
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
