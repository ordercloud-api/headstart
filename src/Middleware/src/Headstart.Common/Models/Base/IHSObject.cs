using System.ComponentModel.DataAnnotations;

namespace Headstart.Models
{
    public interface IHSObject
    {
        [Required]
        [RegularExpression("^[a-zA-Z0-9-_]*$", ErrorMessage = "IDs must have at least 8 characters and no more than 100, are required and can only contain characters Aa-Zz, 0-9, -, and _")]
        [MaxLength(100, ErrorMessage = "Must be a minimum of 8 and maximum of 100 characters")]
        [MinLength(8, ErrorMessage = "Must be a minimum of 8 and maximum of 100 characters")]
        string ID { get; set; }
    }
}
