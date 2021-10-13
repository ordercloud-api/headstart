using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.vertex
{
	public class VertexCalculateTaxRequest
	{
		public VertexSaleMessageType saleMessageType { get; set; }
		//public VertexCurrency currency { get; set; }
		public VertexSeller seller { get; set; }
		public VertexCustomer customer { get; set; }
		//public VertexTaxOverride taxOverride { get; set; }
		//public List<VertexImposition> impositionInclusions { get; set; }
		//public List<VertexJurisdictionOverride> jurisdictionOverrides { get; set; }
		//public VertexSitusOverride situsOverride { get; set; }
		//public VertexDiscount discount { get; set; }
		public List<VertexLineItem> lineItems { get; set; } = new List<VertexLineItem>();
		//public string documentNumber { get; set; }
		public string postingDate { get; set; }
		//public string locationCode { get; set; }
		//public bool returnAssistedParametersIndicator { get; set; }
		//public bool returnGeneratedLineItemsIndicator { get; set; }
		//public VertexDelveryTerm deliveryTerm { get; set; }
		//public DateTime documentDate { get; set; }
		public string transactionId { get; set; }
		public VertexTransactionType transactionType { get; set; }
		//public VertexSimplificationCode simplificationCode { get; set; }
		//public bool isTaxOnlyAdjustmentIndicator { get; set; }
		//public bool postToJournal { get; set; }
		//public DateTime paymentDate { get; set; }
		//public string documentSequenceId { get; set; }
	}

	public class VertexSitusOverride
	{
		public VertexTaxingLocation taxingLocation { get; set; }
	}


	public enum VertexTaxingLocation
	{
		ADMINISTRATIVE_DESTINATION, ADMINISTRATIVE_ORIGIN, DESTINATION, PHYSICAL_ORIGIN
	}

	public class VertexImposition
	{
		public string impositionType { get; set; }
		public VertexJurisdictionLevel value { get; set; }
	}


	public enum VertexTaxOverrideType
	{
		TAXABLE, NONTAXABLE
	}

	public class VertexTaxOverride
	{
		public VertexTaxOverrideType overrideType { get; set; }
		public string overrideReasonCode { get; set; }
	}

	public enum VertexSaleMessageType
	{
		QUOTATION,
		INVOICE,
		DISTRIBUTE_TAX
	}

	public class VertexTaxRegistrations
	{
		public string taxRegistrationNumber { get; set; }
		public string isoCountryCode { get; set; }
		public string mainDivision { get; set; }
		public string hasPhysicalPresenceIndicator { get; set; }
		public string impositionType { get; set; }
	}


	public enum VertexLocationCustomsStatus
	{
		FREE_CIRCULATION, BONDED_WAREHOUSE, FREE_TRADE_ZONE, TEMPORARY_IMPORT, INWARD_PROCESSING_RELIEF, OUTWARD_PROCESSING_RELIEF
	}

	public class VertexExemptionCertificate
	{
		public string exemptionCertificateNumber { get; set; }
		public string value { get; set; }
	}
}
