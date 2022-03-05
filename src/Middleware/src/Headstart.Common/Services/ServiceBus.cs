using System;
using System.Text;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Azure.Messaging.ServiceBus;
using System.Collections.Generic;
using System.Collections.Concurrent;

namespace Headstart.Common.Services
{
    public interface IServiceBus
    {
        Task SendMessage<T>(string queueName, T message, double? afterMinutes = null);
        Task SendMessageBatchToTopicAsync(string queueName, Queue<ServiceBusMessage> messages);
    }

    public class ServiceBus : IServiceBus
    {
        private readonly ConcurrentDictionary<string, ServiceBusSender> senders = new ConcurrentDictionary<string, ServiceBusSender>();
        private readonly ServiceBusClient _client;

        public ServiceBus(AppSettings settings)
        {
            _client = new ServiceBusClient(settings.ServiceBusSettings.ConnectionString);
        }

        public async Task SendMessage<T>(string queueName, T message, double? afterMinutes = null)
        {
            ServiceBusSender sender = senders.GetOrAdd(queueName, _client.CreateSender(queueName));
            string messageString = JsonConvert.SerializeObject(message);
            byte[] messageBytes = Encoding.UTF8.GetBytes(messageString);
            if (afterMinutes == null)
            {
                // send message immediately
                await sender.SendMessageAsync(new ServiceBusMessage(messageBytes));
            }
            else
            {
                // send message after x minutes
                DateTime afterMinutesUtc = DateTime.UtcNow.AddMinutes((double)afterMinutes);
                await sender.SendMessageAsync(new ServiceBusMessage(messageBytes) { ScheduledEnqueueTime = afterMinutesUtc });
            }

        }

        public async Task SendMessageBatchToTopicAsync(string topicName, Queue<ServiceBusMessage> messages)
        {
            ServiceBusSender sender = senders.GetOrAdd(topicName, _client.CreateSender(topicName));

            int messageCount = messages.Count;

            while (messages.Count > 0)
            {
                using ServiceBusMessageBatch messageBatch = await sender.CreateMessageBatchAsync();

                if (messageBatch.TryAddMessage(messages.Peek()))
                {
                    messages.Dequeue();
                }
                else
                {
                    throw new Exception($@"Message {messageCount - messages.Count} is too large and cannot be sent.");
                }

                while (messages.Count > 0 && messageBatch.TryAddMessage(messages.Peek()))
                {
                    messages.Dequeue();
                }
                await sender.SendMessagesAsync(messageBatch);
            }
            Console.WriteLine($@"Sent a batch of {messageCount} messages to the topic: {topicName}.");
        }
    }
}