namespace Headstart.Common.Models.Headstart
{
	public class HsLineItemOrder
	{
		public HsOrder HsOrder { get; set; } = new HsOrder();
		
		public HsLineItem HsLineItem { get; set; } = new HsLineItem();
	}
}