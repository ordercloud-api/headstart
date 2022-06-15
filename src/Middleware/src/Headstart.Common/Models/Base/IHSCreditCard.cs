using System;

namespace Headstart.Common.Models
{
    public interface IHSCreditCard
    {
        string ID { get; set; }

        public string Token { get; set; }

        public DateTimeOffset? DateCreated { get; set; }

        public string CardType { get; set; }

        public string PartialAccountNumber { get; set; }

        public string CardholderName { get; set; }

        public DateTimeOffset? ExpirationDate { get; set; }

        public dynamic xp { get; set; }
    }
}
