using OrderCloud.SDK;
using Headstart.Models.Extended;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace Headstart.Models
{
    public class HSSpec : Spec, IHSObject { }

    public class SpecXp
    {
        [Required]
        public SpecUI UI { get; set; } = new SpecUI();
    }
}