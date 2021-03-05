using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
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
		private readonly IOrderCloudClient _oc;

		public SmartyStreetsCommand(IOrderCloudClient oc, ISmartyStreetsService service)
		{
			_service = service;
			_oc = oc;
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
					if(suggestions.suggestions != null)
                    {
						response.SuggestedAddresses = AddressMapper.Map(suggestions, address);
					}
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
			return await _oc.Me.CreateAddressAsync(validation.ValidAddress, user.AccessToken);
		}

		public async Task<BuyerAddress> SaveMeAddress(string addressID, BuyerAddress address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await _oc.Me.SaveAddressAsync(addressID, validation.ValidAddress, user.AccessToken);
		}

		public async Task PatchMeAddress(string addressID, BuyerAddress patch, VerifiedUserContext user)
		{
			var current = await _oc.Me.GetAddressAsync<BuyerAddress>(addressID, user.AccessToken);
			var patched = PatchHelper.PatchObject(patch, current);
			await ValidateAddress(patched);
			await _oc.Me.PatchAddressAsync(addressID, (PartialBuyerAddress)patch, user.AccessToken);
		}

		// BUYER endpoints
		public async Task<Address> CreateBuyerAddress(string buyerID, Address address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await _oc.Addresses.CreateAsync(buyerID, validation.ValidAddress, user.AccessToken);
		}

		public async Task<Address> SaveBuyerAddress(string buyerID, string addressID, Address address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await _oc.Addresses.SaveAsync(buyerID, addressID, validation.ValidAddress, user.AccessToken);
		}

		public async Task<Address> PatchBuyerAddress(string buyerID, string addressID, Address patch, VerifiedUserContext user)
		{
			var current = await _oc.Addresses.GetAsync<Address>(buyerID, addressID, user.AccessToken);
			var patched = PatchHelper.PatchObject(patch, current);
			await ValidateAddress(patched);
			return await _oc.Addresses.PatchAsync(buyerID, addressID, patch as PartialAddress, user.AccessToken);
		}

		// SUPPLIER endpoints
		public async Task<Address> CreateSupplierAddress(string supplierID, Address address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await _oc.SupplierAddresses.CreateAsync(supplierID, validation.ValidAddress, user.AccessToken);
		}

		public async Task<Address> SaveSupplierAddress(string supplierID, string addressID, Address address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await _oc.SupplierAddresses.SaveAsync(supplierID, addressID, validation.ValidAddress, user.AccessToken);
		}

		public async Task<Address> PatchSupplierAddress(string supplierID, string addressID, Address patch, VerifiedUserContext user)
		{
			var current = await _oc.SupplierAddresses.GetAsync<Address>(supplierID, addressID, user.AccessToken);
			var patched = PatchHelper.PatchObject(patch, current);
			await ValidateAddress(patched);
			return await _oc.SupplierAddresses.PatchAsync(supplierID, addressID, patch as PartialAddress, user.AccessToken);
		}

		// ADMIN endpoints
		public async Task<Address> CreateAdminAddress(Address address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await _oc.AdminAddresses.CreateAsync(address, user.AccessToken);
		}

		public async Task<Address> SaveAdminAddress(string addressID, Address address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await _oc.AdminAddresses.SaveAsync(addressID, validation.ValidAddress, user.AccessToken);
		}

		public async Task<Address> PatchAdminAddress(string addressID, Address patch, VerifiedUserContext user)
		{
			var current = await _oc.AdminAddresses.GetAsync<Address>(addressID, user.AccessToken);
			var patched = PatchHelper.PatchObject(patch, current);
			await ValidateAddress(patched);
			return await _oc.AdminAddresses.PatchAsync(addressID, patch as PartialAddress, user.AccessToken);
		}

		// ORDER endpoints
		public async Task<Order> SetBillingAddress(OrderDirection direction, string orderID, Address address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await _oc.Orders.SetBillingAddressAsync(direction, orderID, validation.ValidAddress, user.AccessToken);
		}

		public async Task<Order> SetShippingAddress(OrderDirection direction, string orderID, Address address, VerifiedUserContext user)
		{
			var validation = await ValidateAddress(address);
			return await _oc.Orders.SetShippingAddressAsync(direction, orderID, validation.ValidAddress, user.AccessToken);
		}
		#endregion
	}
}
