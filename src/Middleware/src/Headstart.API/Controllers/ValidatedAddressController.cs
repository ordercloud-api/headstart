using Headstart.Models.Attributes;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using ordercloud.integrations.smartystreets;
using Headstart.API.Controllers;

namespace Headstart.Common.Controllers
{
	[DocComments("\"Integration\" represents Validated Addresses with suggestions")]
	[HSSection.Integration(ListOrder = 5)]
	public class ValidatedAddressController: HeadstartController
	{
		private readonly ISmartyStreetsCommand _command;
		public ValidatedAddressController(ISmartyStreetsCommand command, AppSettings settings) : base(settings)
		{
			_command = command;
		}

		// ME endpoints
		[HttpPost, Route("me/addresses"), OrderCloudIntegrationsAuth(ApiRole.MeAddressAdmin)]
		public async Task<BuyerAddress> CreateMeAddress([FromBody] BuyerAddress address) =>
			await _command.CreateMeAddress(address, Context);
	

		[HttpPut, Route("me/addresses/{addressID}"), OrderCloudIntegrationsAuth(ApiRole.MeAddressAdmin)]
		public async Task<BuyerAddress> SaveMeAddress(string addressID, [FromBody] BuyerAddress address) =>
			await _command.SaveMeAddress(addressID, address, Context);


		[HttpPatch, Route("me/addresses/{addressID}"), OrderCloudIntegrationsAuth(ApiRole.MeAddressAdmin)]
		public async Task PatchMeAddress(string addressID, [FromBody] BuyerAddress patch) =>
			await _command.PatchMeAddress(addressID, patch, Context);


		// BUYER endpoints
		[HttpPost, Route("buyers/{buyerID}/addresses"), OrderCloudIntegrationsAuth(ApiRole.AddressAdmin)]
		public async Task<Address> CreateBuyerAddress(string buyerID, [FromBody] Address address) =>
			await _command.CreateBuyerAddress(buyerID, address, Context);


		[HttpPut, Route("buyers/{buyerID}/addresses/{addressID}"), OrderCloudIntegrationsAuth(ApiRole.AddressAdmin)]
		public async Task<Address> SaveBuyerAddress(string buyerID, string addressID, [FromBody] Address address) =>
			await _command.SaveBuyerAddress(buyerID, addressID, address, Context);



		[HttpPatch, Route("buyers/{buyerID}/addresses/{addressID}"), OrderCloudIntegrationsAuth(ApiRole.AddressAdmin)]
		public async Task<Address> PatchBuyerAddress(string buyerID, string addressID, [FromBody] Address patch) =>
			await _command.PatchBuyerAddress(buyerID, addressID, patch, Context);


		// SUPPLIER endpoints
		[HttpPost, Route("suppliers/{supplierID}/addresses"), OrderCloudIntegrationsAuth(ApiRole.SupplierAddressAdmin)]
		public async Task<Address> CreateSupplierAddress(string supplierID, [FromBody] Address address) =>
			await _command.CreateSupplierAddress(supplierID, address, Context);


		[HttpPut, Route("suppliers/{supplierID}/addresses/{addressID}"), OrderCloudIntegrationsAuth(ApiRole.SupplierAddressAdmin)]
		public async Task<Address> SaveSupplierAddress(string supplierID, string addressID, [FromBody] Address address) =>
			await _command.SaveSupplierAddress(supplierID, addressID, address, Context);


		[HttpPatch, Route("suppliers/{supplierID}/addresses/{addressID}"), OrderCloudIntegrationsAuth(ApiRole.SupplierAddressAdmin)]
		public async Task<Address> PatchSupplierAddress(string supplierID, string addressID, [FromBody] Address patch) =>
			await _command.PatchSupplierAddress(supplierID, addressID, patch, Context);


		// ADMIN endpoints
		[HttpPost, Route("addresses"), OrderCloudIntegrationsAuth(ApiRole.AdminAddressAdmin)]
		public async Task<Address> CreateAdminAddress([FromBody] Address address) =>
			await _command.CreateAdminAddress(address, Context);


		[HttpPut, Route("addresses/{addressID}"), OrderCloudIntegrationsAuth(ApiRole.AdminAddressAdmin)]
		public async Task<Address> SaveAdminAddress(string addressID, [FromBody] Address address) =>
			await _command.SaveAdminAddress(addressID, address, Context);


		[HttpPatch, Route("addresses/{addressID}"), OrderCloudIntegrationsAuth(ApiRole.AdminAddressAdmin)]
		public async Task<Address> PatchAdminAddress(string addressID, [FromBody] Address patch) =>
			await _command.PatchAdminAddress(addressID, patch, Context);


		// ORDER endpoints
		[HttpPut, Route("order/{direction}/{orderID}/billto"), OrderCloudIntegrationsAuth(ApiRole.Shopper, ApiRole.OrderAdmin)]
		public async Task<Order> SetBillingAddress(OrderDirection direction, string orderID, [FromBody] Address address) =>
			await _command.SetBillingAddress(direction, orderID, address, Context);


		[HttpPut, Route("order/{direction}/{orderID}/shipto"), OrderCloudIntegrationsAuth(ApiRole.Shopper, ApiRole.OrderAdmin)]
		public async Task<Order> SetShippingAddress(OrderDirection direction, string orderID, [FromBody] Address address) =>
			await _command.SetShippingAddress(direction, orderID, address, Context);

	}
}
