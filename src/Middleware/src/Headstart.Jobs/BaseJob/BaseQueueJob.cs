using Microsoft.Azure.ServiceBus;
using Microsoft.Azure.ServiceBus.Core;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Headstart.Jobs
{
    public abstract class BaseQueueJob<T> : BaseJob
    {
        private const int MaxDeadLetterDescriptionLength = 4096;
        protected virtual int RetryTemporaryErrorsAfterMinutes { get; } = 10;
        private Random jitterer = new Random();

        public async Task Run(ILogger logger, Message message, MessageReceiver messageReceiver, MessageSender messageSender)
        {
            _logger = logger;

            string messageString = Encoding.UTF8.GetString(message?.Body);
            T jsonMessage = JsonConvert.DeserializeObject<T>(messageString);

            var result = await TryProcessJobAsync(jsonMessage);
            var lockToken = message.SystemProperties.LockToken;
            switch (result)
            {
                case ResultCode.Success:
                    logger.LogInformation($"Completed the message {message.MessageId} due to successful handling.");
                    break;
                case ResultCode.TemporaryFailure:
                    int resubmitCount = message.UserProperties.ContainsKey("ResubmitCount") ? (int)message.UserProperties["ResubmitCount"] : 0;
                    if(resubmitCount > 5)
                    {
                        await messageReceiver.DeadLetterAsync(lockToken, "Exceeded max retries", GetDeadLetterDescription());
                        logger.LogInformation("$Dead lettered the message due to exceeding max retries, this will need to be retried manually.");
                    } else
                    {
                        await ResubmitMessageAsync(message, messageSender, resubmitCount);
                        logger.LogInformation($"Resubmitted the message {message.MessageId} to be tried again in {RetryTemporaryErrorsAfterMinutes} minutes");
                    }
                    break;
                case ResultCode.PermanentFailure:
                    await messageReceiver.DeadLetterAsync(lockToken, "Permanent failure", GetDeadLetterDescription());
                    logger.LogInformation("$Dead lettered the message due to a permanent failure, this will need to be retried manually.");
                    break;
                default:
                    break;
            }
            LogProgress();
            if (result != ResultCode.Success)
            {
                // throw an error so the function fails and we can see it as an error in azure functions monitor logs
                throw new Exception("There were one or more errors during job");
            }
        }

        private string GetDeadLetterDescription()
        {
            // truncate the description if it exceeds max
            var message = Failed.LastOrDefault();
            if(message.Length < MaxDeadLetterDescriptionLength)
            {
                return message;
            }
            var truncatedSuffix = "...MESSAGE WAS TRUNCATED";
            var truncatedMessage = message.Substring(0, MaxDeadLetterDescriptionLength - truncatedSuffix.Length);
            return truncatedMessage + truncatedSuffix;
        }

        private async Task ResubmitMessageAsync(Message message, MessageSender sender, int resubmitCount)
        {
            // https://markheath.net/post/defer-processing-azure-service-bus-message
            var clone = message.Clone();
            clone.UserProperties["ResubmitCount"] = resubmitCount + 1;
            clone.ScheduledEnqueueTimeUtc = DateTime.UtcNow
                .AddMinutes(RetryTemporaryErrorsAfterMinutes)
                .AddSeconds(jitterer.Next(0, 120)); // plus some jitter up to 2 minutes
            await sender.SendAsync(clone);
        }

        private async Task<ResultCode> TryProcessJobAsync(T message)
        {
            try
            {
                return await ProcessJobAsync(message);
            } catch(Exception ex)
            {
                LogFailure($"Unhandled exception in ProcessJobAsync - {ex.Message} {ex.InnerException.Message} {ex.StackTrace}");
                return ResultCode.PermanentFailure;
            }
        }

        protected abstract Task<ResultCode> ProcessJobAsync(T message);
    }
}
