using OrderCloud.SDK;
using SmartyStreets.USStreetApi;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;

namespace ordercloud.integrations.smartystreets
{
	// TODO - exact same logic as AddressMapper - any way to merge?
	public static class BuyerAddressMapper
	{
		public static Lookup MapToUSStreetLookup(BuyerAddress address)
		{
			var lookup = new Lookup()
			{
				Street = address.Street1,
				Street2 = address.Street2,
				City = address.City,
				State = address.State,
				ZipCode = address.Zip,
				MatchStrategy = Lookup.STRICT,
				MaxCandidates = 5
			};
			return lookup;
		}

		public static List<BuyerAddress> Map(AutoCompleteResponse response, BuyerAddress raw)
		{
			var addresses = response.suggestions.Select(suggestion => {
				var rawCopy = JsonSerializer.Deserialize<BuyerAddress>(JsonSerializer.Serialize(raw));
				rawCopy.Street1 = suggestion.street_line;
				rawCopy.Street2 = suggestion.secondary;
				rawCopy.City = suggestion.city;
				rawCopy.State = suggestion.state;
				rawCopy.Zip = suggestion.zipcode;
				rawCopy.xp = null;
				return rawCopy;
			}).ToList();
			return addresses;
		}

		public static BuyerAddress Map(Candidate candidate, BuyerAddress raw)
		{
			// break reference which was causing pass by reference error
			var rawCopy = JsonSerializer.Deserialize<BuyerAddress>(JsonSerializer.Serialize(raw));
			rawCopy.Street1 = candidate.DeliveryLine1;
			rawCopy.Street2 = candidate.DeliveryLine2;
			rawCopy.City = candidate.Components.CityName;
			rawCopy.State = candidate.Components.State;
			rawCopy.Zip = $"{candidate.Components.ZipCode}-{candidate.Components.Plus4Code}";
			return rawCopy;
		}
	}
}
