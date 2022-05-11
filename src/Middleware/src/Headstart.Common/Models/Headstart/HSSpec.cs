using Headstart.Models.Extended;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace Headstart.Models
{
    
    public class HSSpec : Spec, IHSObject
    {
       
    }

    
    public class SpecXp
    {
        [Required]
        public SpecUI UI { get; set; } = new SpecUI();
    }

    
}
