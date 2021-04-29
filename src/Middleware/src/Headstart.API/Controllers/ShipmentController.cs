using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Attributes;
using ordercloud.integrations.library;
using Headstart.Common.Services.ShippingIntegration.Models;
using Microsoft.AspNetCore.Http;
using Headstart.API.Commands;
using OrderCloud.Catalyst;

namespace Headstart.Common.Controllers
{
    [DocComments("\"Headstart Shipments\" for making shipments in seller app")]
    [HSSection.Headstart(ListOrder = 2)]
    [Route("shipment")]
    public class ShipmentController : BaseController
    {
        
        private readonly IShipmentCommand _command;
        public ShipmentController(IShipmentCommand command)
        {
            _command = command;
        }

        [DocName("POST Headstart Shipment")]
        // todo update auth
        [HttpPost, OrderCloudUserAuth(ApiRole.ShipmentAdmin)]
        public async Task<SuperHSShipment> Create([FromBody] SuperHSShipment superShipment)
        {
            // ocAuth is the token for the organization that is specified in the AppSettings

            // todo add auth to make sure suppliers are creating shipments for their own orders
            return await _command.CreateShipment(superShipment, UserContext);
        } 

        [DocName("POST Batch Shipment Update")]
        [Route("batch/uploadshipment")]
        [HttpPost, OrderCloudUserAuth(ApiRole.ShipmentAdmin)]
        public async Task<BatchProcessResult> UploadShipments([FromForm] FileUpload fileRequest)
        {
            return  await _command.UploadShipments(fileRequest?.File, UserContext);
        }
    }

    public class FileUpload
    {
        public IFormFile File { get; set; }
    }
}
