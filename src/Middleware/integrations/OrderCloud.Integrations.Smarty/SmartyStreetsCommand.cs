using System.Threading.Tasks;
using Headstart.Common.Commands;
using Headstart.Common.Models;
using OrderCloud.Catalyst;
using OrderCloud.Integrations.Smarty.Exceptions;
using OrderCloud.Integrations.Smarty.Mappers;
using OrderCloud.Integrations.Smarty.Models;
using OrderCloud.SDK;

namespace OrderCloud.Integrations.Smarty
{
    public class SmartyStreetsCommand : IAddressValidationCommand
    {
        private readonly ISmartyStreetsService service;
        private readonly IOrderCloudClient oc;
        private readonly SmartyStreetsConfig settings;

        public SmartyStreetsCommand(SmartyStreetsConfig settings, IOrderCloudClient oc, ISmartyStreetsService service)
        {
            this.settings = settings;
            this.service = service;
            this.oc = oc;
        }

        public async Task<AddressValidation> ValidateAddress(Address address)
        {
            var response = new AddressValidation(address);
            if (address.Country == "US")
            {
                var lookup = AddressMapper.MapToUSStreetLookup(address);
                var candidate = await service.ValidateSingleUSAddress(lookup); // Always seems to return 1 or 0 candidates
                if (candidate.Count > 0)
                {
                    response.ValidAddress = AddressMapper.Map(candidate[0], address);

                    // https://www.smarty.com/docs/cloud/us-street-api#analysis
                    response.GapBetweenRawAndValid = candidate[0].Analysis.DpvFootnotes;
                }
                else
                {
                    // no valid address found
                    var suggestions = await service.USAutoCompletePro($"{address.Street1} {address.Street2}");
                    if (suggestions.suggestions != null)
                    {
                        response.SuggestedAddresses = AddressMapper.Map(suggestions, address);
                    }
                }

                if (!response.ValidAddressFound)
                {
                    throw new InvalidAddressException(response);
                }
            }
            else
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
                var candidate = await service.ValidateSingleUSAddress(lookup); // Always seems to return 1 or 0 candidates
                if (candidate.Count > 0)
                {
                    response.ValidAddress = BuyerAddressMapper.Map(candidate[0], address);

                    // https://www.smarty.com/docs/cloud/us-street-api#analysis
                    response.GapBetweenRawAndValid = candidate[0].Analysis.DpvFootnotes;
                }
                else
                {
                    // no valid address found
                    var suggestions = await service.USAutoCompletePro($"{address.Street1} {address.Street2}");
                    if (NoAddressSuggestions(suggestions))
                    {
                        throw new InvalidBuyerAddressException(response);
                    }

                    response.SuggestedAddresses = BuyerAddressMapper.Map(suggestions, address);
                }

                if (!response.ValidAddressFound)
                {
                    throw new InvalidBuyerAddressException(response);
                }
            }
            else
            {
                response.ValidAddress = address;
            }

            return response;
        }

        // ME endpoints
        public async Task<BuyerAddress> CreateMeAddress(BuyerAddress address, DecodedToken decodedToken)
        {
            var validation = await ValidateAddress(address);
            return await oc.Me.CreateAddressAsync(validation.ValidAddress, decodedToken.AccessToken);
        }

        public async Task<BuyerAddress> SaveMeAddress(string addressID, BuyerAddress address, DecodedToken decodedToken)
        {
            var validation = await ValidateAddress(address);
            return await oc.Me.SaveAddressAsync(addressID, validation.ValidAddress, decodedToken.AccessToken);
        }

        public async Task PatchMeAddress(string addressID, BuyerAddress patch, DecodedToken decodedToken)
        {
            var current = await oc.Me.GetAddressAsync<BuyerAddress>(addressID, decodedToken.AccessToken);
            var patched = PatchHelper.PatchObject(patch, current);
            await ValidateAddress(patched);
            await oc.Me.PatchAddressAsync(addressID, (PartialBuyerAddress)patch, decodedToken.AccessToken);
        }

        // BUYER endpoints
        public async Task<Address> CreateBuyerAddress(string buyerID, Address address, DecodedToken decodedToken)
        {
            var validation = await ValidateAddress(address);
            return await oc.Addresses.CreateAsync(buyerID, validation.ValidAddress, decodedToken.AccessToken);
        }

        public async Task<Address> SaveBuyerAddress(string buyerID, string addressID, Address address, DecodedToken decodedToken)
        {
            var validation = await ValidateAddress(address);
            return await oc.Addresses.SaveAsync(buyerID, addressID, validation.ValidAddress, decodedToken.AccessToken);
        }

        public async Task<Address> PatchBuyerAddress(string buyerID, string addressID, Address patch, DecodedToken decodedToken)
        {
            var current = await oc.Addresses.GetAsync<Address>(buyerID, addressID, decodedToken.AccessToken);
            var patched = PatchHelper.PatchObject(patch, current);
            await ValidateAddress(patched);
            return await oc.Addresses.PatchAsync(buyerID, addressID, patch as PartialAddress, decodedToken.AccessToken);
        }

        // SUPPLIER endpoints
        public async Task<Address> CreateSupplierAddress(string supplierID, Address address, DecodedToken decodedToken)
        {
            var validation = await ValidateAddress(address);
            return await oc.SupplierAddresses.CreateAsync(supplierID, validation.ValidAddress, decodedToken.AccessToken);
        }

        public async Task<Address> SaveSupplierAddress(string supplierID, string addressID, Address address, DecodedToken decodedToken)
        {
            var validation = await ValidateAddress(address);
            return await oc.SupplierAddresses.SaveAsync(supplierID, addressID, validation.ValidAddress, decodedToken.AccessToken);
        }

        public async Task<Address> PatchSupplierAddress(string supplierID, string addressID, Address patch, DecodedToken decodedToken)
        {
            var current = await oc.SupplierAddresses.GetAsync<Address>(supplierID, addressID, decodedToken.AccessToken);
            var patched = PatchHelper.PatchObject(patch, current);
            await ValidateAddress(patched);
            return await oc.SupplierAddresses.PatchAsync(supplierID, addressID, patch as PartialAddress, decodedToken.AccessToken);
        }

        // ADMIN endpoints
        public async Task<Address> CreateAdminAddress(Address address, DecodedToken decodedToken)
        {
            var validation = await ValidateAddress(address);
            return await oc.AdminAddresses.CreateAsync(address, decodedToken.AccessToken);
        }

        public async Task<Address> SaveAdminAddress(string addressID, Address address, DecodedToken decodedToken)
        {
            var validation = await ValidateAddress(address);
            return await oc.AdminAddresses.SaveAsync(addressID, validation.ValidAddress, decodedToken.AccessToken);
        }

        public async Task<Address> PatchAdminAddress(string addressID, Address patch, DecodedToken decodedToken)
        {
            var current = await oc.AdminAddresses.GetAsync<Address>(addressID, decodedToken.AccessToken);
            var patched = PatchHelper.PatchObject(patch, current);
            await ValidateAddress(patched);
            return await oc.AdminAddresses.PatchAsync(addressID, patch as PartialAddress, decodedToken.AccessToken);
        }

        // ORDER endpoints
        public async Task<Order> SetBillingAddress(OrderDirection direction, string orderID, Address address, DecodedToken decodedToken)
        {
            var validation = await ValidateAddress(address);
            return await oc.Orders.SetBillingAddressAsync(direction, orderID, validation.ValidAddress, decodedToken.AccessToken);
        }

        public async Task<Order> SetShippingAddress(OrderDirection direction, string orderID, Address address, DecodedToken decodedToken)
        {
            var validation = await ValidateAddress(address);
            return await oc.Orders.SetShippingAddressAsync(direction, orderID, validation.ValidAddress, decodedToken.AccessToken);
        }

        private bool NoAddressSuggestions(AutoCompleteResponse suggestions)
        {
            return suggestions == null || suggestions.suggestions == null || suggestions.suggestions.Count == 0;
        }
    }
}
