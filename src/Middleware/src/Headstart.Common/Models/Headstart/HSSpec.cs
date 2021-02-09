using Headstart.Models.Extended;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using RequiredAttribute = System.ComponentModel.DataAnnotations.RequiredAttribute;

namespace Headstart.Models
{
    [SwaggerModel]
    public class HSSpec : Spec, IHSObject
    {
       
    }

    [SwaggerModel]
    public class SpecXp
    {
        [Required]
        public SpecUI UI { get; set; } = new SpecUI();
    }

    
}
