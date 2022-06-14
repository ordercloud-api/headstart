using System.ComponentModel.DataAnnotations;

namespace OrderCloud.Integrations.EnvironmentSeed.Models
{
    public class AdminAccountCredentials : AccountCredentials
    {
        [Required]
        [RegularExpression("^(?=.{10,100}$)(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*\\W).*$", ErrorMessage = "Password must contain one number, one uppercase letter, one lowercase letter, one special character and have a minimum of 10 characters total")]
        public override string Password { get; set; }
    }
}
