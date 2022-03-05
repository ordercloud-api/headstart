using OrderCloud.SDK;
using System.Collections.Generic;

namespace Headstart.Models
{
    public class HSUser : User<UserXp>, IHSObject { }

    public class UserXp
    {
        public string Country { get; set; } = string.Empty;

        public string OrderEmails { get; set; } = string.Empty;

        public string RequestInfoEmails { get; set; } = string.Empty;

        public List<string> AddtlRcpts { get; set; } = new List<string>();
    }
}