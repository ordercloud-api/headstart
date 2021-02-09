using Headstart.Models;
using OrderCloud.SDK;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using System;
using ordercloud.integrations.library.Cosmos;
using System.Collections.Generic;
using Headstart.Common.Helpers;
using Headstart.Common.Services.CMS;
using Headstart.Common.Services.CMS.Models;
using Headstart.Common;
using Headstart.API.Commands.Crud;

namespace Headstart.API.Commands
{
    public interface INotificationCommand
    {
        Task<SuperHSProduct> CreateModifiedMonitoredSuperProductNotification(MonitoredProductFieldModifiedNotification notification, VerifiedUserContext user);
        Task<SuperHSProduct> UpdateMonitoredSuperProductNotificationStatus(Document<MonitoredProductFieldModifiedNotification> document, string supplierID, string productID, VerifiedUserContext user);
        Task<ListPage<Document<MonitoredProductFieldModifiedNotification>>> ReadMonitoredSuperProductNotificationList(SuperHSProduct product, VerifiedUserContext user);
    }
    public class NotificationCommand : INotificationCommand
    {
        private readonly IOrderCloudClient _oc;
        private readonly AppSettings _settings;
        private readonly ICMSClient _cms;
        private readonly IHSProductCommand _productCommand;
        private readonly ISupplierApiClientHelper _apiClientHelper;
        private readonly string _documentSchemaID = "MonitoredProductFieldModifiedNotification";

        public NotificationCommand(IOrderCloudClient oc, AppSettings settings, ICMSClient cms, IHSProductCommand productCommand, ISupplierApiClientHelper apiClientHelper)
        {
            _oc = oc;
            _settings = settings;
            _cms = cms;
            _productCommand = productCommand;
            _apiClientHelper = apiClientHelper;
        }
        public async Task<SuperHSProduct> CreateModifiedMonitoredSuperProductNotification(MonitoredProductFieldModifiedNotification notification, VerifiedUserContext user)
        {
            if (notification == null || notification?.Product == null) { throw new Exception("Unable to process notification with no product"); }
            var _product = await _oc.Products.PatchAsync(notification.Product.ID, new PartialProduct { Active = false }, user.AccessToken);
            var document = new Document<MonitoredProductFieldModifiedNotification>();
            document.Doc = notification;
            document.ID = $"{notification.Product.ID}_{CosmosInteropID.New()}";
            // Create notifictaion in the cms
            await _cms.Documents.Create(_documentSchemaID, document, await GetAdminToken());
            // Assign the notification to the product
            // TODO: this doesn't work because need to own thing being assigned to AND have DocumentAdmin and we don't want to give suppliers DocumentAdmin
            // await _cms.Documents.SaveAssignment("MonitoredProductFieldModifiedNotification", new DocumentAssignment() { DocumentID = document.ID, ResourceType = ResourceType.Products, ResourceID = _product.ID }, user.AccessToken);
            return await _productCommand.Get(_product.ID, user.AccessToken);
        }

        public async Task<ListPage<Document<MonitoredProductFieldModifiedNotification>>> ReadMonitoredSuperProductNotificationList(SuperHSProduct product, VerifiedUserContext user)
        {
            var token = await GetAdminToken();
            ListArgs<Document<MonitoredProductFieldModifiedNotification>> args;
            var queryParams = new Tuple<string, string>("ID", $"{product.Product.ID}_*");

            args = new ListArgs<Document<MonitoredProductFieldModifiedNotification>>()
            {
                PageSize = 100
            };
            args.Filters.Add(new ListFilter()
            {
                QueryParams = new List<Tuple<string, string>> { queryParams }
            });

            var document = await GetDocumentsByPageAsync(args, token);

            return document;
        }

        private async Task<ListPage<Document<MonitoredProductFieldModifiedNotification>>> GetDocumentsByPageAsync(ListArgs<Document<MonitoredProductFieldModifiedNotification>> args, string token)
        {
            var result = new ListPage<Document<MonitoredProductFieldModifiedNotification>>();

            result.Items = new List<Document<MonitoredProductFieldModifiedNotification>>();

            //Get first 2 pages to make sure no notifications are missing
            for (int pageNumber = 1; pageNumber <= 2; pageNumber++)
            {
                args.Page = pageNumber;
                var documentForPage = await _cms.Documents.List(_documentSchemaID, args, token);

                if (documentForPage.Items.Count > 0)
                {
                    ((List<Document<MonitoredProductFieldModifiedNotification>>)result.Items).AddRange(documentForPage.Items);
                    if (pageNumber == 1)
                    {
                        //Get meta for first item batch
                        result.Meta = documentForPage.Meta;
                    }
                }
            }
            return result;
        }

        public async Task<SuperHSProduct> UpdateMonitoredSuperProductNotificationStatus(Document<MonitoredProductFieldModifiedNotification> document, string supplierID, string productID, VerifiedUserContext user)
        {
            HSProduct product = null;
            HSProduct patchedProduct = null;
            var token = await GetAdminToken();
            try
            {
                product = await _oc.Products.GetAsync<HSProduct>(productID);

            }
            catch (OrderCloudException ex)
            {
                //Product was deleted after it was updated. Delete orphaned notification 
                if (ex.HttpStatus == System.Net.HttpStatusCode.NotFound)
                {
                    await _cms.Documents.Delete(_documentSchemaID, document.ID, token);
                    return new SuperHSProduct();
                }
            }
            if (document.Doc.Status == NotificationStatus.ACCEPTED)
            {
                var supplierClient = await _apiClientHelper.GetSupplierApiClient(supplierID, user.AccessToken);
                if (supplierClient == null) { throw new Exception($"Default supplier client not found. SupplierID: {supplierID}, ProductID: {productID}"); }

                var configToUse = new OrderCloudClientConfig
                {
                    ApiUrl = user.ApiUrl,
                    AuthUrl = user.AuthUrl,
                    ClientId = supplierClient.ID,
                    ClientSecret = supplierClient.ClientSecret,
                    GrantType = GrantType.ClientCredentials,
                    Roles = new[]
                               {
                                     ApiRole.SupplierAdmin,
                                     ApiRole.ProductAdmin
                                },
                };
                try
                {
                    await ClientHelper.RunAction(configToUse, async x =>
                    {
                        patchedProduct = await x.Products.PatchAsync<HSProduct>(productID, new PartialProduct() { Active = true });
                    }
                    );
                }
                catch (OrderCloudException ex)
                {
                    if (ex?.Errors?[0]?.Data != null)
                    {
                        throw new Exception($"Unable to re-activate product: {ex?.Errors?[0]?.Message}: {ex?.Errors?[0]?.Data.ToJRaw()}");
                    }
                    throw new Exception($"Unable to re-activate product: {ex?.Errors?.ToJRaw()}");
                }

                //Delete document after acceptance
                await _cms.Documents.Delete(_documentSchemaID, document.ID, token);
            }
            else
            {
                await _cms.Documents.Save(_documentSchemaID, document.ID, document, token);
            }
            var superProduct = await _productCommand.Get(productID, token);
            superProduct.Product = patchedProduct;
            return superProduct;
        }

        private async Task<string> GetAdminToken()
        {
            var adminOcToken = _oc.TokenResponse?.AccessToken;
            if (adminOcToken == null || DateTime.UtcNow > _oc.TokenResponse.ExpiresUtc)
            {
                await _oc.AuthenticateAsync();
                adminOcToken = _oc.TokenResponse.AccessToken;
            }
            return adminOcToken;
        }
    }
}
