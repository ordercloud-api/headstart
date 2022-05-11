using Headstart.API.Commands;
using Headstart.Common.Services.ShippingIntegration.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using System.Threading.Tasks;

namespace Headstart.API.Controllers
{
	[Route("shipment")]
	public class ShipmentController : CatalystController
	{
		private readonly IShipmentCommand _command;

		/// <summary>
		/// The IOC based constructor method for the ShipmentController class object with Dependency Injection
		/// </summary>
		/// <param name="command"></param>
		public ShipmentController(IShipmentCommand command)
		{
			_command = command;
		}

		/// <summary>
		/// Creates the Shipment item request (POST method)
		/// </summary>
		/// <param name="superShipment"></param>
		/// <returns>The newly created SuperHSShipment object</returns>
		[HttpPost, OrderCloudUserAuth(ApiRole.ShipmentAdmin)]
		public async Task<SuperHSShipment> Create([FromBody] SuperHSShipment superShipment)
		{
			// ocAuth is the token for the organization that is specified in the AppSettings
			// todo add auth to make sure suppliers are creating shipments for their own orders
			return await _command.CreateShipment(superShipment, UserContext);
		}

		/// <summary>
		/// Upload action for Shipment items request (POST method)
		/// </summary>
		/// <param name="fileRequest"></param>
		/// <returns>The upload created BatchProcessResult object</returns>
		[HttpPost, Route("batch/uploadshipment"), OrderCloudUserAuth(ApiRole.ShipmentAdmin)]
		[ApiExplorerSettings(IgnoreApi = true)]
		public async Task<BatchProcessResult> UploadShipments([FromForm] FileUpload fileRequest)
		{
			return await _command.UploadShipments(fileRequest?.File, UserContext);
		}
	}

	public class FileUpload
	{
		public IFormFile File { get; set; }
	}
}