namespace Headstart.Common.Repositories.Models
{
	public class ProductDetailInfo
	{
		public decimal LineTotal { get; set; }
		public string ProductId { get; set; } = string.Empty;
		public double Quantity { get; set; }
		public string Name { get; set; } = string.Empty;
	}
}