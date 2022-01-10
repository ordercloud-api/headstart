using Headstart.Models.Attributes;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using ordercloud.integrations.smartystreets;
using OrderCloud.Catalyst;

namespace Headstart.Common.Controllers
{
	public class ValidatedAddressController: CatalystController
	{
		private readonly ISmartyStreetsCommand _command;
		public ValidatedAddressController(ISmartyStreetsCommand command)
		{
			_command = command;
		}

		// ME endpoints
		[HttpPost, Route("me/addresses"), OrderCloudUserAuth(ApiRole.MeAddressAdmin)]
		public async Task<BuyerAddress> CreateMeAddress([FromBody] BuyerAddress address) =>
			await _command.CreateMeAddress(address, UserContext);

		[HttpPost, Route("me/addresses/validate")]
		public async Task<BuyerAddress> ValidateAddress([FromBody] BuyerAddress address)
        {
			var validation = await _command.ValidateAddress(address);
			return validation.ValidAddress;
        }

		[HttpPut, Route("me/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.MeAddressAdmin)]
		public async Task<BuyerAddress> SaveMeAddress(string addressID, [FromBody] BuyerAddress address) =>
			await _command.SaveMeAddress(addressID, address, UserContext);


		[HttpPatch, Route("me/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.MeAddressAdmin)]
		public async Task PatchMeAddress(string addressID, [FromBody] BuyerAddress patch) =>
			await _command.PatchMeAddress(addressID, patch, UserContext);


		// BUYER endpoints
		[HttpPost, Route("buyers/{buyerID}/addresses"), OrderCloudUserAuth(ApiRole.AddressAdmin)]
		public async Task<Address> CreateBuyerAddress(string buyerID, [FromBody] Address address) =>
			await _command.CreateBuyerAddress(buyerID, address, UserContext);


		[HttpPut, Route("buyers/{buyerID}/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.AddressAdmin)]
		public async Task<Address> SaveBuyerAddress(string buyerID, string addressID, [FromBody] Address address) =>
			await _command.SaveBuyerAddress(buyerID, addressID, address, UserContext);



		[HttpPatch, Route("buyers/{buyerID}/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.AddressAdmin)]
		public async Task<Address> PatchBuyerAddress(string buyerID, string addressID, [FromBody] Address patch) =>
			await _command.PatchBuyerAddress(buyerID, addressID, patch, UserContext);


		// SUPPLIER endpoints
		[HttpPost, Route("suppliers/{supplierID}/addresses"), OrderCloudUserAuth(ApiRole.SupplierAddressAdmin)]
		public async Task<Address> CreateSupplierAddress(string supplierID, [FromBody] Address address) =>
			await _command.CreateSupplierAddress(supplierID, address, UserContext);


		[HttpPut, Route("suppliers/{supplierID}/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.SupplierAddressAdmin)]
		public async Task<Address> SaveSupplierAddress(string supplierID, string addressID, [FromBody] Address address) =>
			await _command.SaveSupplierAddress(supplierID, addressID, address, UserContext);


		[HttpPatch, Route("suppliers/{supplierID}/addresses/{addressID}"), OrderCloudUserAuth(ApiRole.SupplierAddressAdmin)]
		public async Task<Address> PatchSupplierAddress(string supplierID, string addressID, [FromBody] Address patch) =>
			await _command.PatchSupplierAddress(supplierID, addressID, patch, UserContext);


		// ADMIN endpoints
		[HttpPost, Route("addresses"), OrderCloudUserAuth(ApiRole.AdminAddressAdmin)]
		public async Task<Address> CreateAdminAddress([FromBody] Address address) =>
			await _command.CreateAdminAddress(address, UserContext);


		[HttpPut, Route("addresses/{addressID}"), OrderCloudUserAuth(ApiRole.AdminAddressAdmin)]
		public async Task<Address> SaveAdminAddress(string addressID, [FromBody] Address address) =>
			await _command.SaveAdminAddress(addressID, address, UserContext);


		[HttpPatch, Route("addresses/{addressID}"), OrderCloudUserAuth(ApiRole.AdminAddressAdmin)]
		public async Task<Address> PatchAdminAddress(string addressID, [FromBody] Address patch) =>
			await _command.PatchAdminAddress(addressID, patch, UserContext);


		// ORDER endpoints
		[HttpPut, Route("order/{direction}/{orderID}/billto"), OrderCloudUserAuth(ApiRole.Shopper, ApiRole.OrderAdmin)]
		public async Task<Order> SetBillingAddress(OrderDirection direction, string orderID, [FromBody] Address address) =>
			await _command.SetBillingAddress(direction, orderID, address, UserContext);


		[HttpPut, Route("order/{direction}/{orderID}/shipto"), OrderCloudUserAuth(ApiRole.Shopper, ApiRole.OrderAdmin)]
		public async Task<Order> SetShippingAddress(OrderDirection direction, string orderID, [FromBody] Address address) =>
			await _command.SetShippingAddress(direction, orderID, address, UserContext);

	}
}
