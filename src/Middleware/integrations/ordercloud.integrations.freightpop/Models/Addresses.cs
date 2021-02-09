using System.ComponentModel.DataAnnotations;

namespace ordercloud.integrations.freightpop { 
    // the address for getting rates is a condensed
    // version of the address used for order importing
public class RateAddress
    {
        [Required]

        public string City { get; set; }
        [Required]

        public string Country { get; set; }
        [Required]

        public string Street { get; set; }
        [Required]

        public string State { get; set; }
        [Required]

        public string Zip { get; set; }
    }

    // API docs do not indicate that Street1 is required
    // but I think required needs to be enforced on this
    public class OrderAddress
    {
        public string AttentionTo { get; set; }
        [Required]
        public string City { get; set; }
        public string CompanyId { get; set; }
        public string Company { get; set; }
        [Required]
        public string Country { get; set; }
        public string Email { get; set; }
        [Required]
        public string State { get; set; }
        [Required]
        public string Street1 { get; set; }
        public string Street2 { get; set; }
        public string Phone { get; set; }
        [Required]
        public string Zip { get; set; }
    }
}
