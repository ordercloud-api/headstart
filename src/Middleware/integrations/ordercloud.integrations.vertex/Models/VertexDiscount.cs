namespace ordercloud.integrations.vertex
{
    public class VertexDiscount
    {
        public double discountValue { get; set; }

        public VertexDiscountType discountType { get; set; }

        public string userDefinedDiscountCode { get; set; }
    }

    public enum VertexDiscountType
    {
        DiscountAmount,
        DiscountPercent,
    }
}
