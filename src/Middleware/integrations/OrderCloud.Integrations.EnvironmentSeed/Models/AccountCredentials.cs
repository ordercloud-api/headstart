using System.ComponentModel.DataAnnotations;

namespace OrderCloud.Integrations.EnvironmentSeed.Models
{
    public class AccountCredentials
    {
        [Required]
        public string Username { get; set; }

        [Required]
        public virtual string Password { get; set; }
    }
}
