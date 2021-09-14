using Headstart.API.Commands.Zoho;
using Headstart.Common;
using Headstart.Common.Models;
using Headstart.Common.Services;
using OrderCloud.SDK;
using System;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.ServiceBus.Core;
using Newtonsoft.Json;

namespace Headstart.Jobs
{
    public class ZohoJob : BaseQueueJob<ZohoQueueMessage>
    {
        private readonly AppSettings _settings;
        private readonly IZohoCommand _zoho;

        public ZohoJob(AppSettings settings, IZohoCommand zoho)
        {
            _settings = settings;
            _zoho = zoho;
        }

        protected override async Task<ResultCode> ProcessJobAsync(ZohoQueueMessage message)
        {
            try
            {
                LogInformation($"Processing OrderID: {message.OrderID}");
                await _zoho.CreateSalesOrder(message.OrderID);
                return ResultCode.Success;
            }
            catch (ZohoException ex)
            {
                LogFailure($"Zoho Error: {ex.Message} Zoho Error Code: {ex.ErrorCode} {ex.StackTrace}");
                return IsTransientError(ex.HttpStatus) ? ResultCode.TemporaryFailure : ResultCode.PermanentFailure;
            }
            catch (OrderCloudException ex)
            {
                LogFailure($"{ex.InnerException?.Message} {ex?.InnerException?.InnerException?.Message} { JsonConvert.SerializeObject(ex.Errors) } {ex.StackTrace }");
                return IsTransientError(ex.HttpStatus) ? ResultCode.TemporaryFailure : ResultCode.PermanentFailure;
            }
            catch (Exception ex)
            {
                LogFailure($"{ex.Message} {ex?.InnerException?.Message} {ex.StackTrace}");
                return ResultCode.PermanentFailure;
            }
        }

        private bool IsTransientError(HttpStatusCode? status)
        {
            return
                status == null ||
                status == HttpStatusCode.InternalServerError ||
                status == HttpStatusCode.RequestTimeout ||
                status == HttpStatusCode.TooManyRequests;
        }
    }
}
