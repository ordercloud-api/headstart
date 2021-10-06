using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.vertex
{
	public class VertexLineItem
	{
		public VertexSeller seller { get; set; }
		public VertexCustomer customer { get; set; }
		public VertexTaxOverride taxOverride { get; set; }
		public List<VertexImposition> impositionInclusions { get; set; }
		public List<VertexJurisdictionOverride> jurisdictionOverrides { get; set; }
		public VertexSitusOverride situsOverride { get; set; }
		/// <summary>
		/// A code from the host system that identifies the product, material, service, or SKU number. You can use the Vertex Central user interface to map your products to Product Categories. If the supplied Product and Product Class information is not recognized by the calculation engine, a general category indicating TPP is assigned. Required (1) if no Product Class information is supplied. If both are supplied, Product information supersedes Product Class information.
		/// </summary>
		public VertexProduct product { get; set; }
		/// <summary>
		/// A standardized, unique code for the product or service.
		/// </summary>
		public VertexComodityCode commodityCode { get; set; }
		public VertexMeasure quantity { get; set; }
		public VertexMeasure weight { get; set; }
		public VertexMeasure volume { get; set; }
		public double previousTaxPaid { get; set; }
		public double freight { get; set; }
		public double fairMarketValue { get; set; }
		public double cost { get; set; }
		public double unitPrice { get; set; }
		public double extendedPrice { get; set; }
		public double landedCost { get; set; }
		public double inputTotalTax { get; set; }
		public VertexDiscount discount { get; set; }
		public double amountBilledToDate { get; set; }

		public VertexFlexibleFields flexibleFields { get; set; }
		/// <summary>
		/// A line number for the line item. This number can be used as a reference in reports or in audits. If no line item number is supplied, a number will be assigned.
		/// </summary>
		public int lineItemNumber { get; set; }
		/// <summary>
		/// The date on which the line item record was produced. This can also be represented as a PO line item date, a lease line item date, and so on. Defaults to the documentDate of the transaction.
		/// </summary>
		public DateTime taxDate { get; set; }
		/// <summary>
		/// Indicates whether the calculation engine calculates component-level max tax, where applicable. Amounts at the child level of the multi-component item are used for tax calculation. Amounts at the parent level are ignored. Set this attribute to true for line items at the parent level only. Line items at the child level remain false. Defaults to false.
		/// </summary>
		public bool isMulticomponent { get; set; }
		/// <summary>
		/// A value that can be used for tax return filing in jurisdictions that require taxes to be filed for individual retail locations.
		/// </summary>
		public string locationCode { get; set; }
		/// <summary>
		/// An identifier that determines the vendor or customer jurisdiction in which the title transfer of a supply takes place. This is also known as Shipping Terms. Delivery Terms information could be critical to determine the place of supply, for example, in distance selling.
		/// </summary>
		public VertexDelveryTerm deliveryTerm { get; set; }
		/// <summary>
		/// The date when the transaction is posted for auditing and reporting purposes. If no date is provided, the Calculation Engine uses the date when the transaction was processed.
		/// </summary>
		public DateTime postingDate { get; set; }
		/// <summary>
		/// A value that indicates a specific area of an organization generally related to functional or accounting areas.
		/// </summary>
		public string costCenter { get; set; }
		/// <summary>
		/// A unique identifier that associates a line item to a department for purchasing purposes.
		/// </summary>
		public string departmentCode { get; set; }
		/// <summary>
		/// A value that is used with company's Chart of Accounts.
		/// </summary>
		public string generalLedgerAccount { get; set; }
		/// <summary>
		/// A value that represents a specific item or product.
		/// </summary>
		public string meterialCode { get; set; }
		/// <summary>
		/// A unique identifier that associates a line item to a project for purchasing purposes.
		/// </summary>
		public string projectNumber { get; set; }
		/// <summary>
		/// Directly identifies a usage by user-defined usage code.
		/// </summary>
		public string usage { get; set; }
		/// <summary>
		/// Directly identifies a usage class by user-defined usage class code.
		/// </summary>
		public string usageClass { get; set; }
		/// <summary>
		/// A value that represents a specific item or product SKU number.
		/// </summary>
		public string vendorSKU { get; set; }
		/// <summary>
		/// An identifier that further defines the line item and corresponds to the transaction stored in the host system. This parameter is needed to perform synchronization services, but it is not used for reporting purposes.
		/// </summary>
		public string lineItemId { get; set; }
		/// <summary>
		/// Indicates whether tax is included in the Extended Price.
		/// </summary>
		public bool taxIncludedIndicator { get; set; }
		/// <summary>
		/// An identifier for the specific transaction type to be used by the transaction and/or line item. If transactionType is not set at the line item level, the line item uses the transaction level value.
		/// </summary>
		public VertexTransactionType transactionType { get; set; }
		/// <summary>
		/// A code that indicates a special action on the transaction being processed.
		/// </summary>
		public VertexSimplificationCode simplificationCode { get; set; }
		/// <summary>
		///  A contractual term indicating where the title to goods actually changes hands from seller to buyer. This indicator is evaluated to determine the situs (or place of supply) for the transaction.
		/// </summary>
		public VertexTitleTransfer titleTransfer { get; set; }
		/// <summary>
		/// Identifies a phase within a series of transactions in which multiple invoices are raised with only a single movement of goods.
		/// </summary>
		public VertexTransactionPhase chainTransactionPhase { get; set; }
		/// <summary>
		/// The export procedure (sales) or import procedure (purchases) that is applied for INTRASTAT purposes.
		/// </summary>
		public string exportProcedure { get; set; }
	}

	public class VertexMeasure
	{
		public string unitOfMeasure { get; set; }
		public double vlaue { get; set; }
	}

	public class VertexComodityCode
	{
		public string commodityCodeType { get; set; }
		public string value { get; set; }
	}

	public class VertexProduct
	{
		public string productClass { get; set; }
		public string value { get; set; }
	} 

	public enum VertexTransactionType
	{
		SALE, RENTAL, LEASE
	}

	public enum VertexTransactionPhase
	{
		MANUFACTURER, INTERMEDIARY, FINAL
	}

	public enum VertexTitleTransfer
	{
		ORIGIN, IN_TRANSIT, DESTINATION
	}

	public enum VertexSimplificationCode
	{
		CONSIGNMENT, CALL_OFF, TRIANGULATION, REGISTRATION_GROUP
	}

	public enum VertexDelveryTerm
	{
		EXW, FCA, FAS, FOB, CFR, CIF, CPT, CIP, DDP, DAP, DAT, SUP, CUS
	}

	public class VertexFlexibleFields
	{
		/// <summary>
		/// User-defined field for string values.
		/// </summary>
		public List<VertexFlexibleCodeField> flexibleCodeFields { get; set; } = new List<VertexFlexibleCodeField>();
		/// <summary>
		/// User-defined field for double values.
		/// </summary>
		public List<VertexFlexibleNumericField> flexibleNumericFields { get; set; } = new List<VertexFlexibleNumericField>();
		/// <summary>
		/// User-defined field for date values.
		/// </summary>
		public List<VertexFlexibleDateField> flexibleDateFields { get; set; } = new List<VertexFlexibleDateField>();
	}

	public class VertexFlexibleCodeField
	{
		/// <summary>
		/// An identifier for a Flexible Code Field.
		/// </summary>
		public int fieldId { get; set; }

		public string value { get; set; }
	}

	public class VertexFlexibleNumericField
	{
		/// <summary>
		/// An identifier for a Flexible Numeric Field.
		/// </summary>
		public int fieldId { get; set; }

		public double value { get; set; }
	}

	public class VertexFlexibleDateField
	{
		/// <summary>
		/// An identifier for a Flexible Date Field.
		/// </summary>
		public int fieldId { get; set; }

		public DateTime value { get; set; }
	}
}
