using System.Threading.Tasks;
using Headstart.Common.Models;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
{
    public interface IAddressValidationCommand
    {
        Task<AddressValidation> ValidateAddress(Address address);

        Task<BuyerAddressValidation> ValidateAddress(BuyerAddress address);

        // ME endpoints
        Task<BuyerAddress> CreateMeAddress(BuyerAddress address, DecodedToken decodedToken);

        Task<BuyerAddress> SaveMeAddress(string addressID, BuyerAddress address, DecodedToken decodedToken);

        Task PatchMeAddress(string addressID, BuyerAddress patch, DecodedToken decodedToken);

        // BUYER endpoints
        Task<Address> CreateBuyerAddress(string buyerID, Address address, DecodedToken decodedToken);

        Task<Address> SaveBuyerAddress(string buyerID, string addressID, Address address, DecodedToken decodedToken);

        Task<Address> PatchBuyerAddress(string buyerID, string addressID, Address patch, DecodedToken decodedToken);

        // SUPPLIER endpoints
        Task<Address> CreateSupplierAddress(string supplierID, Address address, DecodedToken decodedToken);

        Task<Address> SaveSupplierAddress(string supplierID, string addressID, Address address, DecodedToken decodedToken);

        Task<Address> PatchSupplierAddress(string supplierID, string addressID, Address patch, DecodedToken decodedToken);

        // ADMIN endpoints
        Task<Address> CreateAdminAddress(Address address, DecodedToken decodedToken);

        Task<Address> SaveAdminAddress(string addressID, Address address, DecodedToken decodedToken);

        Task<Address> PatchAdminAddress(string addressID, Address patch, DecodedToken decodedToken);

        // ORDER endpoints
        Task<Order> SetBillingAddress(OrderDirection direction, string orderID, Address address, DecodedToken decodedToken);

        Task<Order> SetShippingAddress(OrderDirection direction, string orderID, Address address, DecodedToken decodedToken);
    }
}
