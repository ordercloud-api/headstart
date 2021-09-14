using Azure.Messaging.ServiceBus;
using Headstart.Common.Services;
using Headstart.Models;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OrderCloud.Catalyst;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Headstart.Jobs
{
    public class SendRecentOrdersJob : BaseTimerJob
    {
        private readonly IOrderCloudClient _oc;
        private readonly IServiceBus _serviceBus;

        public SendRecentOrdersJob(IOrderCloudClient oc, IServiceBus serviceBus)
        {
            _oc = oc;
            _serviceBus = serviceBus;
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
                ["IsSubmitted"] = true
            };

            var orders = await _oc.Orders.ListAllAsync<HSOrder>(OrderDirection.Incoming, filters: filters);
            _logger.LogInformation($"Found {orders.Count} orders with recent changes");

            Queue<ServiceBusMessage> messages = new Queue<ServiceBusMessage>();
            foreach (var order in orders)
            {
                var serializedOrderID = JsonConvert.SerializeObject(order.ID);
                messages.Enqueue(new ServiceBusMessage(serializedOrderID));
            }
            await _serviceBus.SendMessageBatchToTopicAsync("orderreports", messages);
        }
    }
}
