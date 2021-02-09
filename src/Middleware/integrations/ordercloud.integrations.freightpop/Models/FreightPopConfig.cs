using System.ComponentModel.DataAnnotations;

namespace ordercloud.integrations.freightpop { 
    public class FreightPopConfig
    {
        [Required]

        public string Username { get; set; }
        [Required]

        public string Password { get; set; }
        [Required]

        public string BaseUrl { get; set; }
    }
}
