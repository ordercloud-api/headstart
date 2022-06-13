using System.ComponentModel.DataAnnotations;

namespace OrderCloud.Integrations.EnvironmentSeed.Models
{
    public class AccountCredentials
    {
        /// <summary>
        /// The username for the admin user you will log in with after seeding.
        /// </summary>
        [Required]
        public string Username { get; set; }

        /// <summary>
        /// The password for the admin user you will log in with after seeding.
        /// </summary>
        [Required]
        [RegularExpression("^(?=.{10,100}$)(?=.*[a-z])(?=.*[A-Z])(?=.*[0-9])(?=.*\\W).*$", ErrorMessage = "Password must contain one number, one uppercase letter, one lowercase letter, one special character and have a minimum of 10 characters total")]
        public string Password { get; set; }
    }
}
