using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.vertex
{
	public class VertexResponseLineItem : VertexLineItem
	{
		public double totalTax { get; set; }
		public List<VertexTax> taxes { get; set; } = new List<VertexTax>();
	}

	public class VertexTax
	{
		/// <summary>
		/// The name of jurisdiction to which a tax is applied.
		/// </summary>
		public VertexJurisdiction jurisdiction { get; set; }
		/// <summary>
		/// Amount of tax calculated by the calculation engine.
		/// </summary>
		public double calculatedTax { get; set; }
		/// <summary>
		/// For Buyer Input tax and Seller Import tax, this rate is calculated based on the Extended Price and Tax Amount (Import or Input) passed in the Request message. If you total the Extended Price and Tax Amounts before passing them in, this rate is an average.
		/// </summary>
		public double effectiveRate { get; set; }
		/// <summary>
		/// Amount of the line item not subject to tax due to exempt status.
		/// </summary>
		public double exempt { get; set; }
		/// <summary>
		/// Amount of the line item not subject to tax due to nontaxable status.
		/// </summary>
		public double nonTaxable { get; set; }
		/// <summary>
		/// Amount of the line item subject to tax.
		/// </summary>
		public double taxable { get; set; }
		/// <summary>
		/// The name of the imposition to which the relevant tax rule belongs. This is assigned either by Vertex or by the user when setting up a user-defined imposition in the Vertex Central user interface.
		/// </summary>
		public VertexResponseImposition imposition { get; set; }
		/// <summary>
		/// The type description assigned to the imposition. When multiple impositions are imposed within a jurisdiction, each must have a different type. This is assigned either by Vertex or by the user when setting up a user-defined imposition in the Vertex Central user interface.
		/// </summary>
		public VertexResponseImposition impositionType { get; set; }
		public VertexRule taxRuleId { get; set; }
		public VertexRule maxTaxRuleId { get; set; }
		public VertexRule basisRuleId { get; set; }
		public VertexRule recoverableRuleId { get; set; }
		public VertexCertificateNumber certificateNumber { get; set; }
		/// <summary>
		/// The Registration ID for the Seller associated with this line item tax.
		/// </summary>
		public string sellerRegistrationId { get; set; }
		/// <summary>
		/// The Registration ID for the Buyer associated with this line item tax.
		/// </summary>
		public string buyerRegistrationId { get; set; }
		/// <summary>
		/// The Registration ID for the Owner associated with this line item tax.
		/// </summary>
		public string ownerRegistrationId { get; set; }
		public List<int> invoiceTextCodes { get; set; } = new List<int>();
		/// <summary>
		/// Additional information about a tax, to be returned to your host system after tax calculation. To supply a value for this field, you need to set up a post-calculation Tax Assist rule. Note: The text you add to this field is not stored in the Tax Journal.
		/// </summary>
		public string summaryInvoiceText { get; set; }
		/// <summary>
		/// System determination of taxable status based on situs and item type. Note: Tax amounts with a value of Deferred are payable at a later time and should NOT be written to the invoice.
		/// </summary>
		public VertexTaxResult taxResult { get; set; }
		/// <summary>
		/// System determined tax type, based on situs, transaction type, and party role type (perspective)
		/// </summary>
		public VertexTaxType taxType { get; set; }
		/// <summary>
		/// A code that can be used to classify the transaction at the tax level for reporting or tax return filing purposes.
		/// </summary>
		public string vertexTaxCode { get; set; }
		/// <summary>
		/// Indicates that max tax was applied to this line item of the invoice.
		/// </summary>
		public bool maxTaxIndicator { get; set; }
		/// <summary>
		/// The situs determined by the calculation engine for the line item.
		/// </summary>
		public VertexTaxingLocation situs { get; set; }
		/// <summary>
		/// Indicates that the taxpayer company is not registered in the taxing jurisdiction.
		/// </summary>
		public bool notRegisteredIndicator { get; set; }
		/// <summary>
		/// Identifies whether tax is Input VAT, Output VAT, or both according to the perspective of the transaction to allow for Reverse Charges.
		/// </summary>
		public VertexInputOutput inputOutputType { get; set; }
		/// <summary>
		/// A user-defined code for the tax, to be returned to your host system after tax calculation. To return a value for this field, set up a post-calculation Tax Assist rule.
		/// </summary>
		public string taxCode { get; set; }
		/// <summary>
		/// A code that indicates the reason for the exception that was applied to this tax.
		/// </summary>
		public string reasonCode { get; set; }
		public int filingCategoryCode { get; set; }
		/// <summary>
		/// Indicates whether or not the product is a service.
		/// </summary>
		public bool isService { get; set;}
		/// <summary>
		/// A Vertex-defined classification of the applicable tax rule for returns filing purposes. Possible values are: Luxury Rate, Standard Rate, Reduced Rate 1, Reduced Rate 2, Reduced Rate 3, Reduced Rate 4, Reduced Rate 5, User Defined, Zero Rate, Outside the Scope.
		/// </summary>
		public string rateClassification { get; set; }
		/// <summary>
		/// The party who is responsible for the sales tax. Note: Tax amounts with a value of Seller cannot be charged to your customer and should NOT be written to the invoice.
		/// </summary>
		public VertexParty taxCollectedFromParty { get; set; }
		/// <summary>
		/// Specifies the applicable tax structure when the transaction includes a flat fee or quantity-based fee.
		/// </summary>
		public VertexTaxStructure taxStructure { get; set; }
	}

	public enum VertexTaxStructure
	{
		BRACKET, FLAT_TAX, QUANTITY, SINGLE_RATE, TIERED
	}

	public enum VertexParty
	{
		SELLER, BUYER
	}

	public enum VertexInputOutput
	{
		INPUT, IMPORT, OUTPUT, INPUT_OUTPUT
	}

	public enum VertexTaxType
	{
		SALES, SELLER_USE, CONSUMERS_USE, VAT, IMPORT_VAT, NONE
	}

	public enum VertexTaxResult
	{
		TAXABLE, NONTAXABLE, EXEMPT, DPPAPPLIED, NO_TAX, DEFERRED
	}

	public class VertexCertificateNumber
	{
		public string certificateType { get; set; }
		public string value { get; set; }
	}

	public class VertexRule
	{
		/// <summary>
		/// Identifier for user-defined rules. This identifier is useful for troubleshooting. The default value is False, Vertex-defined rule.
		/// </summary>
		public bool userDefined { get; set; }
		/// <summary>
		/// Indicates whether this rule applies as a Sales Tax Holiday.
		/// </summary>
		public bool salesTaxHolidayIndicator { get; set; }
		public string value { get; set; }
	}

	public class VertexResponseImposition
	{
		/// <summary>
		/// Identifier for user-defined rules. This identifier is useful for troubleshooting. The default value is False, Vertex-defined rule.
		/// </summary>
		public bool userDefined { get; set; }
		/// <summary>
		/// The Vertex internal identifier for the impositionType.
		/// </summary>
		public int impositionTypeId { get; set; }
		public string value { get; set; }
	}
}
