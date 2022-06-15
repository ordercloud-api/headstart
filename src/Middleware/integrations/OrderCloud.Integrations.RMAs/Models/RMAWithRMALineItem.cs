namespace OrderCloud.Integrations.RMAs.Models
{
    public class RMAWithRMALineItem
    {
        public RMA RMA { get; set; }

        public RMALineItem RMALineItem { get; set; }
    }
}
