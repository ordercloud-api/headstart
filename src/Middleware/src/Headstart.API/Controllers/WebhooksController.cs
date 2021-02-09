using Headstart.Common.Services;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.SDK;
using ordercloud.integrations.library;
using System;
using Headstart.Common.Models;
using Headstart.Common.Queries;
using Headstart.API.Controllers;
using Headstart.Models.Headstart;

namespace Headstart.Common.Controllers
{
    public class WebhooksController : BaseController
    {
        private readonly AppSettings _settings;
        private readonly ISendgridService _sendgridService;
        private readonly IOrderCloudClient _oc;
        private readonly ResourceHistoryQuery<ProductHistory> _productQuery;
        private readonly ResourceHistoryQuery<PriceScheduleHistory> _priceScheduleQuery;

        public WebhooksController(
            AppSettings settings, 
            ISendgridService sendgridService, 
            ResourceHistoryQuery<ProductHistory> productQuery, 
            ResourceHistoryQuery<PriceScheduleHistory> priceScheduleQuery, 
            IOrderCloudClient orderCloud) : base(settings)
        {
            _settings = settings;
            _sendgridService = sendgridService;
            _productQuery = productQuery;
            _priceScheduleQuery = priceScheduleQuery;
            _oc = orderCloud;
        }

        [HttpPost, Route("productcreated")]
        [OrderCloudWebhookAuth]
        public async void HandleProductCreation([FromBody] HSProductCreatePayload payload)
        {
            var update = new ProductHistory()
            {
                Action = ActionType.Create,
                ResourceID = payload.Response.Body.ID,
                Resource = payload.Response.Body,
            };
            await _productQuery.Post(update);
        }

        [HttpPost, Route("productupdated")]
        [OrderCloudWebhookAuth]
        public async void HandleProductUpdate([FromBody] HSProductUpdatePayload payload)
        {
            Console.WriteLine(payload);
            var update = new ProductHistory()
            {
                Action = ActionType.Update,
                ResourceID = payload.Response.Body.ID,
                Resource = payload.Response.Body,

            };
            await _productQuery.Put(update);
        }

        [HttpPost, Route("priceschedulecreated")]
        [OrderCloudWebhookAuth]
        public async void HandlePriceScheduleCreation([FromBody] WebhookPayloads.PriceSchedules.Create payload)
        {
            var update = new PriceScheduleHistory()
            {
                Action = ActionType.Create,
                ResourceID = payload.Response.Body.ID,
                Resource = payload.Response.Body,
            };
            await _priceScheduleQuery.Post(update);
        }

        [HttpPost, Route("priceScheduleupdated")]
        [OrderCloudWebhookAuth]
        public async void HandlePriceScheduleUpdate([FromBody] WebhookPayloads.PriceSchedules.Patch payload)
        {
            var updatedPriceSchedule = await _oc.PriceSchedules.GetAsync(payload.Request.Body.ID);
            var update = new PriceScheduleHistory()
            {
                Action = ActionType.Update,
                ResourceID = updatedPriceSchedule.ID,
                Resource = updatedPriceSchedule,

            };
            await _priceScheduleQuery.Put(update);
        }


        [HttpPost, Route("supplierupdated")]
        [OrderCloudWebhookAuth]
        public async void HandleSupplierUpdate([FromBody] WebhookPayloads.Suppliers.Patch payload)
        {
            // to mp manager when a supplier is updated
            await _sendgridService.SendSingleEmail(_settings.SendgridSettings.FromEmail, "to", "Supplier Updated", "<h1>this is a test email for supplier update</h1>");
        }
    }
}