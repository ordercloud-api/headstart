using System;
using System.Threading.Tasks;
using Headstart.Common.Models;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
{
    public class AddressValidationCommand : IAddressValidationCommand
    {
        public async Task<AddressValidation> ValidateAddress(Address address)
        {
            return await Task.FromResult(new AddressValidation(address));
        }

        public async Task<BuyerAddressValidation> ValidateAddress(BuyerAddress address)
        {
            return await Task.FromResult(new BuyerAddressValidation(address));
        }
    }
}
