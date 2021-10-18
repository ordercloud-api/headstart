using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.vertex
{
	public class VertexJurisdiction
	{
		/// <summary>
		/// The level of the jurisdiction for which the tax on the line item is applied.
		/// </summary>
		public VertexJurisdictionLevel jurisdictionLevel { get; set; }
		/// <summary>
		/// The Vertex-specific number that identifies a jurisdiction.
		/// </summary>
		public int jurisdictionId { get; set; }
		/// <summary>
		/// The date when the tax for the jurisdiction became effective.
		/// </summary>
		public DateTime effectiveDate { get; set; }
		/// <summary>
		/// The date after which the tax for the jurisdiction is no longer effective.
		/// </summary>
		public string expirationDate { get; set; }
		/// <summary>
		/// Jurisdiction code assigned by the relevant governmental authority.
		/// </summary>
		public string externalJurisdictionCode { get; set; }
		public string value { get; set; }
	}

	public class VertexJurisdictionOverride
	{
		public string impositionType { get; set; }
		public VertexJurisdictionLevel jurisdictionLevel { get; set; }
		public VertexRateOverride rateOverride { get; set; }
		public VertexDeductionOverride deductionOverride { get; set; }
	}

	public enum VertexJurisdictionLevel
	{
		APO, BOROUGH, CITY, COUNTRY, COUNTY, DISTRICT, FPO, LOCAL_IMPROVEMENT_DISTRICT, PARISH, PROVINCE, SPECIAL_PURPOSE_DISTRICT, STATE, TERRITORY, TOWNSHIP, TRADE_BLOCK, TRANSIT_DISTRICT
	}

	public class VertexRateOverride
	{
		public double value { get; set; }
	}

	public class VertexDeductionOverride
	{
	}

}
