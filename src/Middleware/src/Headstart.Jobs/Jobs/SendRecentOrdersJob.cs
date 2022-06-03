using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using Headstart.Common.Models;
using Headstart.Common.Services;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Jobs
{
    public class SendRecentOrdersJob : BaseTimerJob
    {
        private readonly IOrderCloudClient oc;
        private readonly IServiceBus serviceBus;

        public SendRecentOrdersJob(IOrderCloudClient oc, IServiceBus serviceBus)
        {
            this.oc = oc;
            this.serviceBus = serviceBus;
        }

        protected override bool ShouldRun => true;

        protected override async Task ProcessJob()
        {
            _logger.LogInformation($"C# Timer trigger function executed at: {DateTime.Now}");

            var now = DateTime.UtcNow;
            var dateEnd = now.AddMinutes(-15).ToString("s");
            var dateFilter = $">{dateEnd}";

            var filters = new Dictionary<string, object>
            {
                ["LastUpdated"] = $"{dateFilter}",
                ["IsSubmitted"] = true,
            };

            var orders = await oc.Orders.ListAllAsync<HSOrder>(OrderDirection.Incoming, filters: filters);
            _logger.LogInformation($"Found {orders.Count} orders with recent changes");

            Queue<ServiceBusMessage> messages = new Queue<ServiceBusMessage>();
            foreach (var order in orders)
            {
                var serializedOrderID = JsonConvert.SerializeObject(order.ID);
                messages.Enqueue(new ServiceBusMessage(serializedOrderID));
            }

            await serviceBus.SendMessageBatchToTopicAsync("orderreports", messages);
        }
    }
}
