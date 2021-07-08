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

namespace ordercloud.integrations.avalara
{
	public interface IAvalaraCommand
	{
		// Use this before checkout. No records will be saved in avalara.
		Task<TransactionModel> GetEstimateAsync(OrderWorksheet orderWorksheet);
		// Use this during submit.
		Task<TransactionModel> CreateTransactionAsync(OrderWorksheet orderWorksheet);
		// Committing the transaction makes it eligible to be filed as part of a tax return. 
		// When should we do this? 
		Task<TransactionModel> CommitTransactionAsync(string transactionCode);
		Task<ListPage<TaxCode>> ListTaxCodesAsync(ListArgs<TaxCode> hsListArgs);
		Task<TaxCertificate> GetCertificateAsync(int certificateID);
		Task<TaxCertificate> CreateCertificateAsync(TaxCertificate cert, Address buyerLocation);
		Task<TaxCertificate> UpdateCertificateAsync(int certificateID, TaxCertificate cert, Address buyerLocation);
	}

	public enum AppEnvironment { Test, Staging, Production }

	public class AvalaraCommand : IAvalaraCommand
	{
		private readonly IOrderCloudClient _oc;
		private readonly AvalaraConfig _settings;
		private readonly AvaTaxClient _avaTax;
		private readonly string _companyCode;
		private readonly string _baseUrl;
		private bool noAccountCredentials;
		private AppEnvironment appEnvironment;

		public AvalaraCommand(IOrderCloudClient oc, AvalaraConfig settings, AvaTaxClient client, string environment)
		{
			_oc = oc;
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

		public async Task<TransactionModel> GetEstimateAsync(OrderWorksheet orderWorksheet)
		{
			if (ShouldMockAvalaraResponse()) { return CreateMockTransactionModel(); }
			var taxEstimate = await CreateTransactionAsync(DocumentType.SalesOrder, orderWorksheet);
			return taxEstimate;
		}

        private TransactionModel CreateMockTransactionModel()
        {
			TransactionModel result = new TransactionModel()
			{
				totalTax = (decimal?)123.45,
				description = "Mock Avalara Response for Headstart",
				date = DateTime.Now
			};

			return result;
        }

        public async Task<TransactionModel> CreateTransactionAsync(OrderWorksheet orderWorksheet)
		{
			if (ShouldMockAvalaraResponse()) { return CreateMockTransactionModel(); }

			var transaction = await CreateTransactionAsync(DocumentType.SalesInvoice, orderWorksheet);
			return transaction;
		}

		public async Task<TransactionModel> CommitTransactionAsync(string transactionCode)
		{
			if (ShouldMockAvalaraResponse()) { return CreateMockTransactionModel(); }

			var model = new CommitTransactionModel() { commit = true };
			var transaction = await _avaTax.CommitTransactionAsync(_companyCode, transactionCode, DocumentType.SalesInvoice, "", model);
			return transaction;
		}

		public async Task<ListPage<TaxCode>> ListTaxCodesAsync(ListArgs<TaxCode> hsListArgs)
		{
			if (ShouldMockAvalaraResponse()) { return CreateMockTaxCodeList(); }

			var args = TaxCodeMapper.Map(hsListArgs);
			var avataxCodes = await _avaTax.ListTaxCodesAsync(args.Filter, args.Top, args.Skip, args.OrderBy);
			var codeList = TaxCodeMapper.Map(avataxCodes, args);
			return codeList;
		}

        private ListPage<TaxCode> CreateMockTaxCodeList()
        {
			return new ListPage<TaxCode>()
			{
				Items = new List<TaxCode>() { new TaxCode() {Description = "Mock Tax Code for Headstart", Code = "Headstart Tax Code" } },
				Meta = new ListPageMeta() { }
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

		private async Task<TransactionModel> CreateTransactionAsync(DocumentType docType, OrderWorksheet orderWorksheet)
		{
			var standardLineItems = orderWorksheet.LineItems.Where(li => li.Product.xp.ProductType == "Standard")?.ToList();
            if (standardLineItems.Any())
            {
				try
				{
					if (ShouldMockAvalaraResponse()) { return CreateMockTransactionModel(); }

					var promosOnOrder = await _oc.Orders.ListAllPromotionsAsync(OrderDirection.Incoming, orderWorksheet.Order.ID);
					var createTransactionModel = orderWorksheet.ToAvalaraTransactionModel(_companyCode, docType, promosOnOrder);
					var transaction = await _avaTax.CreateTransactionAsync("", createTransactionModel);
					return transaction;

				}
				catch (AvaTaxError e)
				{
					throw new CatalystBaseException("AvalaraTaxError", 400, e.error.error.message, e.error.error);
				}
			} else
            {
				return new TransactionModel
				{
					code = "NotTaxable",
					totalTax = 0
				};
            }
			
		}
	}
}
