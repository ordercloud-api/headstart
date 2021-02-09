using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Headstart.Models;
using Headstart.Models.Attributes;
using ordercloud.integrations.library;
using Headstart.API.Controllers;
using Headstart.Common;

namespace Headstart.Orchestration
{
    [DocComments("\"Orchestration\" represents Organization objects exposed for orchestration control")]
    [HSSection.Orchestration(ListOrder = 2)]
    [Route("orchestration")]
    public class OrchestrationUserController : BaseController
    {
        private readonly IOrchestrationCommand _command;

        public OrchestrationUserController(AppSettings settings, IOrchestrationCommand command) : base(settings)
        {
            _command = command;
        }

        [DocName("POST Buyer")]
        [HttpPost, Route("buyer"), OrderCloudIntegrationsAuth()]
        public async Task<HSBuyer> PostBuyer([FromBody] HSBuyer obj)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, obj.ID);
        }

        [DocName("POST User")]
        [HttpPost, Route("{buyerId}/user"), OrderCloudIntegrationsAuth()]
        public async Task<HSUser> PostUser([FromBody] HSUser obj, string buyerId)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, buyerId);
        }

        [DocName("POST UserGroup")]
        [HttpPost, Route("{buyerId}/usergroup"), OrderCloudIntegrationsAuth()]
        public async Task<HSUserGroup> PostUserGroup([FromBody] HSUserGroup obj, string buyerId)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, buyerId);
        }

        [DocName("POST UserGroupAssignment")]
        [HttpPost, Route("{buyerId}/usergroupassignment"), OrderCloudIntegrationsAuth()]
        public async Task<HSUserGroupAssignment> PostUserGroupAssignment([FromBody] HSUserGroupAssignment obj, string buyerId)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, buyerId);
        }

        [DocName("POST Address")]
        [HttpPost, Route("{buyerId}/address"), OrderCloudIntegrationsAuth()]
        public async Task<HSAddressBuyer> PostAddress([FromBody] HSAddressBuyer obj, string buyerId)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, buyerId);
        }

        [DocName("POST AddressAssignment")]
        [HttpPost, Route("{buyerId}/addressassignment"), OrderCloudIntegrationsAuth()]
        public async Task<HSAddressAssignment> PostAddressAssignment([FromBody] HSAddressAssignment obj, string buyerId)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, buyerId);
        }

        [DocName("POST CostCenter")]
        [HttpPost, Route("{buyerId}/costcenter"), OrderCloudIntegrationsAuth()]
        public async Task<HSCostCenter> PostCostCenter([FromBody] HSCostCenter obj, string buyerId)
        {
            return await _command.SaveToQueue(obj, this.VerifiedUserContext, buyerId);
        }
    }
}
