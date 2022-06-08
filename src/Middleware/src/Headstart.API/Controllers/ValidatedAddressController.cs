using System.Threading.Tasks;
using Headstart.Common.Commands;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers
{
    public class ValidatedAddressController : CatalystController
    {
        private readonly IAddressValidationCommand addressValidationCommand;

        public ValidatedAddressController(IAddressValidationCommand addressValidationCommand)
        {
            this.addressValidationCommand = addressValidationCommand;
        }

        // ME endpoints
        [HttpPost, Route("me/addresses"), OrderCloudUserAuth(ApiRole.MeAddressAdmin)]
        public async Task<BuyerAddress> CreateMeAddress([FromBody] BuyerAddress address) =>
            await addressValidationCommand.CreateMeAddress(address, UserContext);

        [HttpPost, Route("me/addresses/validate")]
        public async Task<BuyerAddress> ValidateAddress([FromBody] BuyerAddress address)
        {
            var validation = await addressValidationCommand.ValidateAddress(address);
            return validation.ValidAddress;
        }

        [HttpPut, Route("me/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.MeAddressAdmin)]
        public async Task<BuyerAddress> SaveMeAddress(string addressID, [FromBody] BuyerAddress address) =>
            await addressValidationCommand.SaveMeAddress(addressID, address, UserContext);

        [HttpPatch, Route("me/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.MeAddressAdmin)]
        public async Task PatchMeAddress(string addressID, [FromBody] BuyerAddress patch) =>
            await addressValidationCommand.PatchMeAddress(addressID, patch, UserContext);

        // BUYER endpoints
        [HttpPost, Route("buyers/{buyerID}/addresses"), OrderCloudUserAuth(ApiRole.AddressAdmin)]
        public async Task<Address> CreateBuyerAddress(string buyerID, [FromBody] Address address) =>
            await addressValidationCommand.CreateBuyerAddress(buyerID, address, UserContext);

        [HttpPut, Route("buyers/{buyerID}/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.AddressAdmin)]
        public async Task<Address> SaveBuyerAddress(string buyerID, string addressID, [FromBody] Address address) =>
            await addressValidationCommand.SaveBuyerAddress(buyerID, addressID, address, UserContext);

        [HttpPatch, Route("buyers/{buyerID}/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.AddressAdmin)]
        public async Task<Address> PatchBuyerAddress(string buyerID, string addressID, [FromBody] Address patch) =>
            await addressValidationCommand.PatchBuyerAddress(buyerID, addressID, patch, UserContext);

        // SUPPLIER endpoints
        [HttpPost, Route("suppliers/{supplierID}/addresses"), OrderCloudUserAuth(ApiRole.SupplierAddressAdmin)]
        public async Task<Address> CreateSupplierAddress(string supplierID, [FromBody] Address address) =>
            await addressValidationCommand.CreateSupplierAddress(supplierID, address, UserContext);

        [HttpPut, Route("suppliers/{supplierID}/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.SupplierAddressAdmin)]
        public async Task<Address> SaveSupplierAddress(string supplierID, string addressID, [FromBody] Address address) =>
            await addressValidationCommand.SaveSupplierAddress(supplierID, addressID, address, UserContext);

        [HttpPatch, Route("suppliers/{supplierID}/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.SupplierAddressAdmin)]
        public async Task<Address> PatchSupplierAddress(string supplierID, string addressID, [FromBody] Address patch) =>
            await addressValidationCommand.PatchSupplierAddress(supplierID, addressID, patch, UserContext);

        // ADMIN endpoints
        [HttpPost, Route("addresses"), OrderCloudUserAuth(ApiRole.AdminAddressAdmin)]
        public async Task<Address> CreateAdminAddress([FromBody] Address address) =>
            await addressValidationCommand.CreateAdminAddress(address, UserContext);

        [HttpPut, Route("addresses/{addressID}"), OrderCloudUserAuth(ApiRole.AdminAddressAdmin)]
        public async Task<Address> SaveAdminAddress(string addressID, [FromBody] Address address) =>
            await addressValidationCommand.SaveAdminAddress(addressID, address, UserContext);

        [HttpPatch, Route("addresses/{addressID}"), OrderCloudUserAuth(ApiRole.AdminAddressAdmin)]
        public async Task<Address> PatchAdminAddress(string addressID, [FromBody] Address patch) =>
            await addressValidationCommand.PatchAdminAddress(addressID, patch, UserContext);

        // ORDER endpoints
        [HttpPut, Route("order/{direction}/{orderID}/billto"), OrderCloudUserAuth(ApiRole.Shopper, ApiRole.OrderAdmin)]
        public async Task<Order> SetBillingAddress(OrderDirection direction, string orderID, [FromBody] Address address) =>
            await addressValidationCommand.SetBillingAddress(direction, orderID, address, UserContext);

        [HttpPut, Route("order/{direction}/{orderID}/shipto"), OrderCloudUserAuth(ApiRole.Shopper, ApiRole.OrderAdmin)]
        public async Task<Order> SetShippingAddress(OrderDirection direction, string orderID, [FromBody] Address address) =>
            await addressValidationCommand.SetShippingAddress(direction, orderID, address, UserContext);
    }
}
