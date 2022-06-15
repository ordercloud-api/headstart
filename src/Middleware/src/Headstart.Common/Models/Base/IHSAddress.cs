using System;

namespace Headstart.Common.Models
{
    public interface IHSAddress
    {
        string ID { get; set; }

        DateTimeOffset? DateCreated { get; set; }

        string CompanyName { get; set; }

        string FirstName { get; set; }

        string LastName { get; set; }

        string Street1 { get; set; }

        string Street2 { get; set; }

        string City { get; set; }

        string State { get; set; }

        string Zip { get; set; }

        string Country { get; set; }

        string Phone { get; set; }

        string AddressName { get; set; }

        dynamic xp { get; set; }
    }
}
