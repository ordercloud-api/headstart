using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.WindowsAzure.Storage.Blob;
using Newtonsoft.Json;
using OrderCloud.SDK;

namespace ordercloud.integrations.library
{
    public class GlobalExceptionHandler : IMiddleware
    {
        private readonly IOrderCloudIntegrationsBlobService _blob;
        public GlobalExceptionHandler(BlobServiceConfig blobconfig)
        {
            _blob = new OrderCloudIntegrationsBlobService(blobconfig);
        }

        public async Task InvokeAsync(HttpContext context, RequestDelegate next)
        {
            try
            {
                await next(context).ConfigureAwait(true);
            }
            catch (Exception ex)
            {
                await HandleExceptionAsync(context, ex);
            }
        }

        private Task HandleExceptionAsync(HttpContext context, Exception ex)
        {
            const HttpStatusCode code = HttpStatusCode.InternalServerError; // 500 if unexpected
            context.Response.ContentType = "application/json";

            switch (ex)
            {
                case OrderCloudIntegrationException intException:
                    context.Response.StatusCode = HttpStatusCode.BadRequest.To<int>();
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(intException.ApiError));
                case OrderCloudException ocException:
                    context.Response.StatusCode = ocException.HttpStatus.To<int>();
                    return context.Response.WriteAsync(JsonConvert.SerializeObject(ocException.Errors));
            }

            // this is only to be hit IF it's not handled properly in the stack. It's considered a bug if ever hits this. that's why it's a 500
            var apiError = new ApiError()
            {
                Data = ex.Message,
                ErrorCode = code.ToString(),
                Message = "Unknown error has occured."
            };
            var userFacingError = JsonConvert.SerializeObject(apiError, Formatting.Indented);

            var log = new ApiErrorLog
            {
                Data = ex.Message,
                ErrorCode = code.ToString(),
                Message = "Unknown error has occured.",
                StackTrace = ex.StackTrace,
                InnerException = ex.InnerException,
                TimeStamp = DateTime.Now
            };
            LogError(log);

            context.Response.StatusCode = (int)code;
            return context.Response.WriteAsync(userFacingError);
        }

        private async Task LogError(ApiErrorLog log)
        {
            // log error to blob
            await _blob.Container.CreateIfNotExistsAsync();
            var today = DateTime.Now.ToString("yyyy/MM/dd");
            var todaysLog = _blob.Container.GetAppendBlobReference($"{today}.txt");
            if (!await todaysLog.ExistsAsync())
            {
                await todaysLog.CreateOrReplaceAsync();
            }
            var ENTRY_DELIMITER = "*** ------------ START NEW ERROR ------------ ***";
            var logEntry = $"{ENTRY_DELIMITER}\r\n{JsonConvert.SerializeObject(log, Formatting.Indented)}\r\n";
            await todaysLog.AppendTextAsync(logEntry);
        }

        public class ApiErrorLog : ApiError
        {
            public string StackTrace { get; set; }
            public Exception InnerException { get; set; }
            public DateTime TimeStamp { get; set; }
        }
    }
}

