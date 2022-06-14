using System.Threading.Tasks;
using Headstart.Common.Models;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
{
    public interface IAddressValidationCommand
    {
        Task<AddressValidation> ValidateAddress(Address address);

        Task<BuyerAddressValidation> ValidateAddress(BuyerAddress address);
    }
}
