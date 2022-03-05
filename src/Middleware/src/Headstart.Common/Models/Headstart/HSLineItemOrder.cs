namespace Headstart.Models.Headstart
{
    public class HSLineItemOrder
    {
        public HSOrder HSOrder { get; set; } = new HSOrder();

        public HSLineItem HSLineItem { get; set; } = new HSLineItem();
    }
}