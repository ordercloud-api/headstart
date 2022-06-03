using Headstart.Common.Models;
using OrderCloud.Integrations.Zoho.Models;

namespace OrderCloud.Integrations.Zoho.Mappers
{
    public static class ZohoAddressMapper
    {
        public static ZohoAddress Map(IHSAddress address)
        {
            return new ZohoAddress()
            {
                attention = address.CompanyName,
                address = address.Street1,
                street2 = address.Street2,
                city = address.City,
                state = address.State,
                zip = address.Zip,
                country = address.Country,
                phone = address.Phone,
                state_code = address.State,
            };
        }
    }
}
