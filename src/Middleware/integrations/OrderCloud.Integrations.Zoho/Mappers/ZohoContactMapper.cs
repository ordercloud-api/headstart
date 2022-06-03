using System.Collections.Generic;
using System.Linq;
using Headstart.Common.Models;
using OrderCloud.Integrations.Library;
using OrderCloud.Integrations.Zoho.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Zoho.Mappers
{
    public static class ZohoContactMapper
    {
        public static ZohoContact Map(HSSupplier supplier, HSAddressSupplier address, User user, ZohoCurrency currency)
        {
            return new ZohoContact()
            {
                company_name = supplier.ID,
                contact_name = supplier.Name,
                contact_type = "vendor",
                billing_address = ZohoAddressMapper.Map(address),
                shipping_address = ZohoAddressMapper.Map(address),
                contact_persons = new List<ZohoContactPerson>()
                {
                    new ZohoContactPerson()
                    {
                        email = user.Email,
                        first_name = user.FirstName,
                        last_name = user.LastName,
                        phone = user.Phone,
                    },
                },
                currency_id = currency.currency_id,
            };
        }

        public static ZohoContact Map(ZohoContact contact, HSSupplier supplier, HSAddressSupplier address, User user, ZohoCurrency currency)
        {
            return new ZohoContact()
            {
                contact_id = contact.contact_id,
                company_name = supplier.ID,
                contact_name = supplier.Name,
                contact_type = "vendor",
                billing_address = ZohoAddressMapper.Map(address),
                shipping_address = ZohoAddressMapper.Map(address),
                contact_persons = contact.contact_persons = (contact.contact_persons != null &&
                                                             contact.contact_persons.Any(c => c.email == user.Email))
                    ? new List<ZohoContactPerson>()
                    {
                        new ZohoContactPerson()
                        {
                            email = user.Email,
                            first_name = user.FirstName,
                            last_name = user.LastName,
                            phone = user.Phone,
                        },
                    }
                    : null,
                currency_id = currency.currency_id,
            };
        }

        public static ZohoContact Map(HSBuyer buyer, IList<HSUser> users, ZohoCurrency currency, HSBuyerLocation location)
        {
            return new ZohoContact()
            {
                company_name = $"{buyer.Name} - {location.Address?.xp.LocationID}",
                contact_name = $"{location.Address?.AddressName} - {location.Address?.xp.LocationID}",
                contact_type = "customer",
                billing_address = ZohoAddressMapper.Map(location.Address),
                shipping_address = ZohoAddressMapper.Map(location.Address),
                contact_persons = ZohoContactMapper.Map(users),
                currency_id = currency.currency_id,
                notes = $"Franchise ID: {buyer.ID} ~ Location ID: {location.Address?.xp.LocationID}",
            };
        }

        public static ZohoContact Map(ZohoContact contact, HSBuyer buyer, IList<HSUser> users, ZohoCurrency currency, HSBuyerLocation location)
        {
            contact.company_name = $"{buyer.Name} - {location.Address?.xp.LocationID}";
            contact.contact_name = $"{location.Address?.AddressName} - {location.Address?.xp.LocationID}";
            contact.contact_type = "customer";
            contact.billing_address = ZohoAddressMapper.Map(location.Address);
            contact.shipping_address = ZohoAddressMapper.Map(location.Address);
            contact.contact_persons = ZohoContactMapper.Map(users, contact);
            contact.currency_id = currency.currency_id;
            contact.notes = $"Franchise ID: {buyer.ID} ~ Location ID: {location.Address?.xp.LocationID}";
            return contact;
        }

        public static List<ZohoContactPerson> Map(IList<HSUser> users, ZohoContact contact = null)
        {
            // there is no property at this time for primary contact in OC, so we'll go with the first in the list
            var list = new List<ZohoContactPerson>();
            foreach (var user in users)
            {
                if (contact?.contact_persons != null && contact.contact_persons.Any(p => p.email == user.Email))
                {
                    var c = contact.contact_persons.FirstOrDefault(p => p.email == user.Email);
                    c.contact_person_id = c.contact_person_id;
                    c.email = user.Email;
                    c.first_name = user.FirstName;
                    c.last_name = user.LastName;
                    c.phone = user.Phone;
                    list.Add(c);
                }
                else
                {
                    list.Add(new ZohoContactPerson()
                    {
                        email = user.Email,
                        first_name = user.FirstName,
                        last_name = user.LastName,
                        phone = user.Phone,
                    });
                }
            }

            return list.DistinctBy(u => u.email).ToList();
        }
    }
}
