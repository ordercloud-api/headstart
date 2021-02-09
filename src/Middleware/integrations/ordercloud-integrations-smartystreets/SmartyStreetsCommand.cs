using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace ordercloud.integrations.smartystreets
{
	public interface ISmartyStreetsCommand
	{
		Task<AddressValidation> ValidateAddress(Address address);
		Task<BuyerAddressValidation> ValidateAddress(BuyerAddress address);
		// ME endpoints
		Task<BuyerAddress> CreateMeAddress(BuyerAddress address, VerifiedUserContext user);
		Task<BuyerAddress> SaveMeAddress(string addressID, BuyerAddress address, VerifiedUserContext user);
		Task PatchMeAddress(string addressID, BuyerAddress patch, VerifiedUserContext user);
		// BUYER endpoints
		Task<Address> CreateBuyerAddress(string buyerID, Address address, VerifiedUserContext user);
		Task<Address> SaveBuyerAddress(string buyerID, string addressID, Address address, VerifiedUserContext user);
		Task<Address> PatchBuyerAddress(string buyerID, string addressID, Address patch, VerifiedUserContext user);
		// SUPPLIER endpoints
		Task<Address> CreateSupplierAddress(string supplierID, Address address, VerifiedUserContext user);
		Task<Address> SaveSupplierAddress(string supplierID, string addressID, Address address, VerifiedUserContext user);
		Task<Address> PatchSupplierAddress(string supplierID, string addressID, Address patch, VerifiedUserContext user);
		// ADMIN endpoints
		Task<Address> CreateAdminAddress(Address address, VerifiedUserContext user);
		Task<Address> SaveAdminAddress(string addressID, Address address, VerifiedUserContext user);
		Task<Address> PatchAdminAddress(string addressID, Address patch, VerifiedUserContext user);
		// ORDER endpoints
		Task<Order> SetBillingAddress(OrderDirection direction, string orderID, Address address, VerifiedUserContext user);
		Task<Order> SetShippingAddress(OrderDirection direction, string orderID, Address address, VerifiedUserContext user);
	}

	public class SmartyStreetsCommand : ISmartyStreetsCommand
	{
		private readonly ISmartyStreetsService _service;
		public SmartyStreetsCommand(ISmartyStreetsService service)
		{
			_service = service;
		}

		public async Task<AddressValidation> ValidateAddress(Address address)
		{
			var response = new AddressValidation(address);
			if (address.Country == "US")
			{
				var lookup = AddressMapper.MapToUSStreetLookup(address);
				var candidate = await _service.ValidateSingleUSAddress(lookup); // Always seems to return 1 or 0 candidates
				if (candidate.Count > 0)
				{
					response.ValidAddress = AddressMapper.Map(candidate[0], address);
					response.GapBetweenRawAndValid = candidate[0].Analysis.DpvFootnotes;
				}
				else
				{
					// no valid address found
					var suggestions = await _service.USAutoCompletePro($"{address.Street1} {address.Street2}");
					response.SuggestedAddresses = AddressMapper.Map(suggestions, address);
				}
				if (!response.ValidAddressFound) throw new InvalidAddressException(response);
            } else
            {
				response.ValidAddress = address;
            }
			return response;
		}

		public async Task<BuyerAddressValidation> ValidateAddress(BuyerAddress address)
		{
			var response = new BuyerAddressValidation(address);
			if (address.Country == "US")
            {
				var lookup = BuyerAddressMapper.MapToUSStreetLookup(address);
				var candidate = await _service.ValidateSingleUSAddress(lookup); // Always seems to return 1 or 0 candidates
				if (candidate.Count > 0)
				{
					response.ValidAddress = BuyerAddressMapper.Map(candidate[0], address);
					response.GapBetweenRawAndValid = candidate[0].Analysis.DpvFootnotes;
				}
				else
				{
					// no valid address found
					var suggestions = await _service.USAutoCompletePro($"{address.Street1} {address.Street2}");
					if (NoAddressSuggestions(suggestions)) throw new InvalidBuyerAddressException(response);
					response.SuggestedAddresses = BuyerAddressMapper.Map(suggestions, address);
				}
				if (!response.ValidAddressFound) throw new InvalidBuyerAddressException(response);
            } else
            {
				response.ValidAddress = address;
			}
			return response;
		}

		private bool NoAddressSuggestions(AutoCompleteResponse suggestions)
        {
			return (suggestions == null || suggestions.suggestions == null || suggestions.suggestions.Count == 0);
        }

		#region Ordercloud Routes
		// ME endpoints
		public async Task<BuyerAddress> CreateMeAddress(BuyerAddress address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await new OrderCloudClientWithContext(user).Me.CreateAddressAsync(validation.ValidAddress);
		}

		public async Task<BuyerAddress> SaveMeAddress(string addressID, BuyerAddress address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await new OrderCloudClientWithContext(user).Me.SaveAddressAsync(addressID, validation.ValidAddress);
		}

		public async Task PatchMeAddress(string addressID, BuyerAddress patch, VerifiedUserContext user)
		{
			var current = await new OrderCloudClientWithContext(user).Me.GetAddressAsync<BuyerAddress>(addressID);
			var patched = PatchHelper.PatchObject(patch, current);
			await ValidateAddress(patched);
			await new OrderCloudClientWithContext(user).Me.PatchAddressAsync(addressID, (PartialBuyerAddress)patch);
		}

		// BUYER endpoints
		public async Task<Address> CreateBuyerAddress(string buyerID, Address address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await new OrderCloudClientWithContext(user).Addresses.CreateAsync(buyerID, validation.ValidAddress);
		}

		public async Task<Address> SaveBuyerAddress(string buyerID, string addressID, Address address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await new OrderCloudClientWithContext(user).Addresses.SaveAsync(buyerID, addressID, validation.ValidAddress);
		}

		public async Task<Address> PatchBuyerAddress(string buyerID, string addressID, Address patch, VerifiedUserContext user)
		{
			var current = await new OrderCloudClientWithContext(user).Addresses.GetAsync<Address>(buyerID, addressID);
			var patched = PatchHelper.PatchObject(patch, current);
			await ValidateAddress(patched);
			return await new OrderCloudClientWithContext(user).Addresses.PatchAsync(buyerID, addressID, patch as PartialAddress);
		}

		// SUPPLIER endpoints
		public async Task<Address> CreateSupplierAddress(string supplierID, Address address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await new OrderCloudClientWithContext(user).SupplierAddresses.CreateAsync(supplierID, validation.ValidAddress);
		}

		public async Task<Address> SaveSupplierAddress(string supplierID, string addressID, Address address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await new OrderCloudClientWithContext(user).SupplierAddresses.SaveAsync(supplierID, addressID, validation.ValidAddress);
		}

		public async Task<Address> PatchSupplierAddress(string supplierID, string addressID, Address patch, VerifiedUserContext user)
		{
			var current = await new OrderCloudClientWithContext(user).SupplierAddresses.GetAsync<Address>(supplierID, addressID);
			var patched = PatchHelper.PatchObject(patch, current);
			await ValidateAddress(patched);
			return await new OrderCloudClientWithContext(user).SupplierAddresses.PatchAsync(supplierID, addressID, patch as PartialAddress);
		}

		// ADMIN endpoints
		public async Task<Address> CreateAdminAddress(Address address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await new OrderCloudClientWithContext(user).AdminAddresses.CreateAsync(address);
		}

		public async Task<Address> SaveAdminAddress(string addressID, Address address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await new OrderCloudClientWithContext(user).AdminAddresses.SaveAsync(addressID, validation.ValidAddress);
		}

		public async Task<Address> PatchAdminAddress(string addressID, Address patch, VerifiedUserContext user)
		{
			var current = await new OrderCloudClientWithContext(user).AdminAddresses.GetAsync<Address>(addressID);
			var patched = PatchHelper.PatchObject(patch, current);
			await ValidateAddress(patched);
			return await new OrderCloudClientWithContext(user).AdminAddresses.PatchAsync(addressID, patch as PartialAddress);
		}

		// ORDER endpoints
		public async Task<Order> SetBillingAddress(OrderDirection direction, string orderID, Address address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await new OrderCloudClientWithContext(user).Orders.SetBillingAddressAsync(direction, orderID, validation.ValidAddress);
		}

		public async Task<Order> SetShippingAddress(OrderDirection direction, string orderID, Address address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await new OrderCloudClientWithContext(user).Orders.SetShippingAddressAsync(direction, orderID, validation.ValidAddress);
		}
		#endregion
	}
}
