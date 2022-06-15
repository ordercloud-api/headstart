using System;
using System.Threading.Tasks;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
{
    public class AddressCommand : IAddressCommand
    {
        private readonly IOrderCloudClient orderCloudClient;

        public AddressCommand(IOrderCloudClient orderCloudClient)
        {
            this.orderCloudClient = orderCloudClient;
        }

        public async Task<Address> CreateAdminAddress(Address address, DecodedToken decodedToken)
        {
            return await orderCloudClient.AdminAddresses.CreateAsync(address, decodedToken.AccessToken);
        }

        public async Task<Address> CreateBuyerAddress(string buyerID, Address address, DecodedToken decodedToken)
        {
            return await orderCloudClient.Addresses.CreateAsync(buyerID, address, decodedToken.AccessToken);
        }

        public async Task<BuyerAddress> CreateMeAddress(BuyerAddress address, DecodedToken decodedToken)
        {
            return await orderCloudClient.Me.CreateAddressAsync(address, decodedToken.AccessToken);
        }

        public async Task<Address> CreateSupplierAddress(string supplierID, Address address, DecodedToken decodedToken)
        {
            return await orderCloudClient.SupplierAddresses.CreateAsync(supplierID, address, decodedToken.AccessToken);
        }

        public Task<Address> PatchAdminAddress(string addressID, Address patch, DecodedToken decodedToken)
        {
            throw new NotImplementedException("Not currently used in buyer and seller apps. Should be reviewed for whether patching should be applied to a PartialAddress.");
        }

        public Task<Address> PatchBuyerAddress(string buyerID, string addressID, Address patch, DecodedToken decodedToken)
        {
            throw new NotImplementedException("Not currently used in buyer and seller apps. Should be reviewed for whether patching should be applied to a PartialAddress.");
        }

        public Task PatchMeAddress(string addressID, BuyerAddress patch, DecodedToken decodedToken)
        {
            throw new NotImplementedException("Not currently used in buyer and seller apps. Should be reviewed for whether patching should be applied to a PartialAddress.");
        }

        public Task<Address> PatchSupplierAddress(string supplierID, string addressID, Address patch, DecodedToken decodedToken)
        {
            throw new NotImplementedException("Not currently used in buyer and seller apps. Should be reviewed for whether patching should be applied to a PartialAddress.");
        }

        public async Task<Address> SaveAdminAddress(string addressID, Address address, DecodedToken decodedToken)
        {
            return await orderCloudClient.AdminAddresses.SaveAsync(addressID, address, decodedToken.AccessToken);
        }

        public async Task<Address> SaveBuyerAddress(string buyerID, string addressID, Address address, DecodedToken decodedToken)
        {
            return await orderCloudClient.Addresses.SaveAsync(buyerID, addressID, address, decodedToken.AccessToken);
        }

        public async Task<BuyerAddress> SaveMeAddress(string addressID, BuyerAddress address, DecodedToken decodedToken)
        {
            return await orderCloudClient.Me.SaveAddressAsync(addressID, address, decodedToken.AccessToken);
        }

        public async Task<Address> SaveSupplierAddress(string supplierID, string addressID, Address address, DecodedToken decodedToken)
        {
            return await orderCloudClient.SupplierAddresses.SaveAsync(supplierID, addressID, address, decodedToken.AccessToken);
        }

        public async Task<Order> SetBillingAddress(OrderDirection direction, string orderID, Address address, DecodedToken decodedToken)
        {
            return await orderCloudClient.Orders.SetBillingAddressAsync(direction, orderID, address, decodedToken.AccessToken);
        }

        public async Task<Order> SetShippingAddress(OrderDirection direction, string orderID, Address address, DecodedToken decodedToken)
        {
            return await orderCloudClient.Orders.SetShippingAddressAsync(direction, orderID, address, decodedToken.AccessToken);
        }
    }
}
