using OrderCloud.SDK;

namespace Headstart.Models
{
    public class HSSpecOption : SpecOption<SpecOptionXp>, IHSObject { }

    public class SpecOptionXp
    {
        public string Description { get; set; } = string.Empty;
        public string SpecID { get; set; } = string.Empty;
    }
}