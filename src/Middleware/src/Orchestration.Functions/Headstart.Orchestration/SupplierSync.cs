using System;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common;
using Headstart.Common.Helpers;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using Headstart.API.Commands;

namespace Headstart.Orchestration
{
    public class SupplierSync
    {
        private readonly IOrderCloudIntegrationsFunctionToken _token;
        private readonly ISupplierSyncCommand _supplier;
        private readonly IProductTemplateCommand _product;
        private readonly AppSettings _settings;

        public SupplierSync(AppSettings settings, IOrderCloudIntegrationsFunctionToken token, ISupplierSyncCommand supplier, IProductTemplateCommand product)
        {
            _settings = settings;
            _token = token;
            _supplier = supplier;
            _product = product;
        }

        [FunctionName("UploadProductTemplate")]
        public async Task<IActionResult> UploadProductTemplate(
            [HttpTrigger(AuthorizationLevel.Anonymous, "POST", Route = "templateupload/{supplierId}")]
            HttpRequest req, string supplierId, ILogger log)
        {
            var user = await _token.Authorize(req, new[] { ApiRole.ProductAdmin, ApiRole.PriceScheduleAdmin });
            Require.That(user.SupplierID == supplierId, new ErrorCode("Authorization.InvalidToken", 401, "Authorization.InvalidToken: Access token is invalid or expired."));

            var form = await req.ReadFormAsync();
            var result = await _product.ParseProductTemplateFlat(form.Files.GetFile("file"), user);
            
            return await Task.FromResult(new OkObjectResult(result));
        }

        [FunctionName("GetSupplierOrder")]
        public async Task<IActionResult> GetSupplierOrder([HttpTrigger(AuthorizationLevel.Anonymous, "GET", Route = "{supplierId}/{orderId}")]
            HttpRequest req, string supplierId, string orderId, ILogger log)
        {
            log.LogInformation($"Supplier Order GET Request: {supplierId} {orderId}");
            try
            {
                var user = await _token.Authorize(req, new[] { ApiRole.OrderAdmin, ApiRole.OrderReader });
                Require.That(user.SupplierID == supplierId, new ErrorCode("Authorization.InvalidToken", 401, "Authorization.InvalidToken: Access token is invalid or expired."));
                var order = await _supplier.GetOrderAsync(orderId, user);
                log.LogInformation($"Supplier Order GET Request Success: {supplierId} {orderId}");
                return new OkObjectResult(order);
            }
            catch (OrderCloudIntegrationException oex)
            {
                log.LogError($"Error retrieving order for supplier: {supplierId} {orderId}. { oex.ApiError }");
                return new BadRequestObjectResult(oex.ApiError);
            }
            catch (OrderCloudException ocex)
            {
                log.LogError($"Error retrieving order for supplier: {supplierId} {orderId}. { ocex.Errors }");
                return new BadRequestObjectResult(ocex.Errors);
            }
            catch (Exception ex)
            {
                log.LogError($"Error retrieving order for supplier: {supplierId} {orderId}. {ex.Message}");
                return new BadRequestObjectResult(new ApiError()
                {
                    ErrorCode = "500",
                    Message = ex.Message
                });
            }
        }
    }
}
