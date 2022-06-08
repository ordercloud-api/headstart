using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Controllers
{
    /// <summary>
    /// Shipments.
    /// </summary>
    [Route("shipment")]
    public class ShipmentController : CatalystController
    {
        private readonly IShipmentCommand shipmentCommand;

        public ShipmentController(IShipmentCommand command)
        {
            this.shipmentCommand = command;
        }

        /// <summary>
        /// POST Headstart Shipment.
        /// </summary>
        // todo update auth
        [HttpPost, OrderCloudUserAuth(ApiRole.ShipmentAdmin)]
        public async Task<SuperHSShipment> Create([FromBody] SuperHSShipment superShipment)
        {
            // ocAuth is the token for the organization that is specified in the AppSettings

            // todo add auth to make sure suppliers are creating shipments for their own orders
            return await shipmentCommand.CreateShipment(superShipment, UserContext);
        }

        /// <summary>
        /// POST Batch Shipment Update.
        /// </summary>
        [HttpPost, Route("batch/uploadshipment"), OrderCloudUserAuth(ApiRole.ShipmentAdmin)]
        [ApiExplorerSettings(IgnoreApi = true)]
        public async Task<BatchProcessResult> UploadShipments([FromForm] FileUpload fileRequest)
        {
            return await shipmentCommand.UploadShipments(fileRequest?.File, UserContext);
        }
    }

    public class FileUpload
    {
        public IFormFile File { get; set; }
    }
}
