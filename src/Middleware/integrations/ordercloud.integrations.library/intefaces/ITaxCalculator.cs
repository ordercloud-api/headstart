using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace ordercloud.integrations.library
{
	/// <summary>
	/// An interface to define expected responses from tax calculation. Meant to be used in OrderCloud checkout integration event listeners.
	/// </summary>
	public interface ITaxCalculator
	{
		/// <summary>
		/// Calculates tax for an order without creating any records. Use this to display tax amount to user prior to order submit.
		/// </summary>
		Task<OrderTaxCalculation> CalculateEstimateAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions);
		/// <summary>
		/// Creates a tax transaction record in the calculating system. Use this once on purchase, payment capture, or fulfillment.
		/// </summary>
		Task<OrderTaxCalculation> CommitTransactionAsync(OrderWorksheet orderWorksheet, List<OrderPromotion> promotions);
	}

	/// <summary>
	/// Represents the details of the tax costs on one Order.
	/// </summary>
	public class OrderTaxCalculation
	{
		/// <summary>
		/// ID of the OrderCloud Order
		/// </summary>
		public string OrderID { get; set; }
		/// <summary>
		/// An ID from the tax calculation system that identifies this transaction or calculation
		/// </summary>
		public string ExternalTransactionID { get; set; }
		/// <summary>
		/// The total tax to be paid by the purchaser on the order
		/// </summary>
		public decimal TotalTax { get; set; }
		/// <summary>
		/// Tax details broken down by line item
		/// </summary>
		public List<LineItemTaxCalculation> LineItems { get; set; } = new List<LineItemTaxCalculation>();
		/// <summary>
		/// Taxes that apply across the order. For example, shipping tax.
		/// </summary>
		public List<TaxDetails> OrderLevelTaxes { get; set; } = new List<TaxDetails>();
	}

	/// <summary>
	/// Represents the details of the tax costs on one LineItem.
	/// </summary>
	public class LineItemTaxCalculation
	{
		/// <summary>
		/// ID of the OrderCloud line item
		/// </summary>
		public string LineItemID { get; set; }
		/// <summary>
		/// The sum of taxes that apply to this line item
		/// </summary>
		public decimal LineItemTotalTax { get; set; }
		/// <summary>
		/// Taxes that apply specifically to this line item
		/// </summary>
		public List<TaxDetails> LineItemLevelTaxes { get; set; } = new List<TaxDetails>();
	}

	/// <summary>
	/// A tax cost levied by one juristdiction and applied to some element of an Order (lineItem, shipping, ect.).
	/// </summary>
	public class TaxDetails
	{
		/// <summary>
		/// The tax to be paid by the purchaser
		/// </summary>
		public decimal Tax { get; set; }
		/// <summary>
		/// The amount of the line item cost on which tax applies
		/// </summary>
		public decimal Taxable { get; set; }
		/// <summary>
		/// The amount of the line item cost on which tax does not apply
		/// </summary>
		public decimal Exempt { get; set; }
		/// <summary>
		/// A description of the tax.
		/// </summary>
		public string TaxDescription { get; set; }
		/// <summary>
		/// The level of the authority collecting tax, e.g. federal, state, city.
		/// </summary>
		public string JurisdictionLevel { get; set; }
		/// <summary>
		/// The name of the authority collecting tax.
		/// </summary>
		public string JurisdictionValue { get; set; }
		/// <summary>
		/// ID of the ship estimate this tax applies to. Null if not a shipping tax.
		/// </summary>
		public string ShipEstimateID { get; set; }
	}
}
