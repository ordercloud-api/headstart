using System.ComponentModel.DataAnnotations;
using ordercloud.integrations.library;

namespace Headstart.Models.Extended
{
    [SwaggerModel]
    public class TaxProperties
    {
        [Required]
        public string Category { get; set; }
        [Required]
        public string Code { get; set; }
        [MaxLength(200, ErrorMessage = "Tax Description cannot exceed 200 characters")]
        public string Description { get; set; }
    }
}
