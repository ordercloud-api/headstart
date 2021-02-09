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

	public class AvalaraCommand : IAvalaraCommand
	{
		private readonly AvalaraConfig _settings;
		private readonly AvaTaxClient _avaTax;
		private readonly string _companyCode;
		private readonly string _baseUrl;

		public AvalaraCommand(AvalaraConfig settings, AvaTaxClient client)
		{
			_settings = settings;
			_companyCode = _settings.CompanyCode;
			_baseUrl = _settings.BaseApiUrl;
            _avaTax = client;
        }

		public async Task<TransactionModel> GetEstimateAsync(OrderWorksheet orderWorksheet)
		{
			var taxEstimate = await CreateTransactionAsync(DocumentType.SalesOrder, orderWorksheet);
			return taxEstimate;
		}

		public async Task<TransactionModel> CreateTransactionAsync(OrderWorksheet orderWorksheet)
		{
			var transaction = await CreateTransactionAsync(DocumentType.SalesInvoice, orderWorksheet);
			return transaction;
		}

		public async Task<TransactionModel> CommitTransactionAsync(string transactionCode)
		{
			var model = new CommitTransactionModel() { commit = true };
			var transaction = await _avaTax.CommitTransactionAsync(_companyCode, transactionCode, DocumentType.SalesInvoice, "", model);
			return transaction;
		}

		public async Task<ListPage<TaxCode>> ListTaxCodesAsync(ListArgs<TaxCode> hsListArgs)
		{
			var args = TaxCodeMapper.Map(hsListArgs);
			var avataxCodes = await _avaTax.ListTaxCodesAsync(args.Filter, args.Top, args.Skip, args.OrderBy);
			var codeList = TaxCodeMapper.Map(avataxCodes, args);
			return codeList;
		}

		public async Task<TaxCertificate> GetCertificateAsync(int certificateID)
		{
			var companyID = _settings.CompanyID;
			var certificate = _avaTax.GetCertificateAsync(companyID, certificateID, "");
			var pdf = GetCertificateBase64String(companyID, certificateID);
			var mappedCertificate = TaxCertificateMapper.Map(await certificate, await pdf);
			return mappedCertificate;
		}

		public async Task<TaxCertificate> CreateCertificateAsync(TaxCertificate cert, Address buyerLocation)
		{
			var companyID = _settings.CompanyID;
			var certificates = await _avaTax.CreateCertificatesAsync(companyID, false, new List<CertificateModel> { 
				TaxCertificateMapper.Map(cert, buyerLocation, companyID) 
			});
			var pdf = await GetCertificateBase64String(companyID, certificates[0].id ?? 0);
			var mappedCertificate = TaxCertificateMapper.Map(certificates[0], pdf);
			return mappedCertificate;
		}

		public async Task<TaxCertificate> UpdateCertificateAsync(int certificateID, TaxCertificate cert, Address buyerLocation)
		{
			var companyID = _settings.CompanyID;
			var certificate = _avaTax.UpdateCertificateAsync(companyID, certificateID, TaxCertificateMapper.Map(cert, buyerLocation, companyID));
			var pdf = GetCertificateBase64String(companyID, certificateID);
			var mappedCertificate = TaxCertificateMapper.Map(await certificate, await pdf);
			return mappedCertificate;
		}

		// The avalara SDK method for this was throwing an internal JSON parse exception.
		private async Task<string> GetCertificateBase64String(int companyID, int certificateID)
		{
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
					var createTransactionModel = orderWorksheet.ToAvalaraTransationModel(_companyCode, docType);
					var transaction = await _avaTax.CreateTransactionAsync("", createTransactionModel);
					return transaction;

				}
				catch (AvaTaxError e)
				{
					throw new OrderCloudIntegrationException(new ApiError
					{
						ErrorCode = "AvalaraTaxError",
						Message = e.error.error.message,
						Data = e.error.error
					});
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
