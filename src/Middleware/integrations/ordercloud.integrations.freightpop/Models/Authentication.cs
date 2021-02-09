using System.ComponentModel.DataAnnotations;

namespace ordercloud.integrations.freightpop
{
    public class PasswordGrantRequestData
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class PasswordGrantResponseData
    {
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string ExpiresIn { get; set; }
    }
}
