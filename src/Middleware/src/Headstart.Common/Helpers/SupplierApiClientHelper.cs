using Headstart.Common.Extensions;
using ordercloud.integrations.library;
using OrderCloud.SDK;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Headstart.Common.Helpers
{
    public interface ISupplierApiClientHelper
    {
        Task<ApiClient> GetSupplierApiClient(string supplierID, string token);
    }
    public class SupplierApiClientHelper : ISupplierApiClientHelper
    {
        private readonly IOrderCloudClient _oc;
        private readonly AppSettings _settings;
        public SupplierApiClientHelper(AppSettings settings, IOrderCloudClient oc)
        {
            _settings = settings;
            _oc = oc;
        }
        public async Task<ApiClient> GetSupplierApiClient(string supplierID, string token)
        {
            Supplier supplierDetails = await _oc.Suppliers.GetAsync(supplierID);
            // GET ApiClient using the xp value on supplier
            try
            {
                ApiClient apiClient = await _oc.ApiClients.GetAsync(supplierDetails?.xp?.ApiClientID);
                // If ApiClient exists, return it
                if (apiClient?.ID == null)
                {
                    // in some cases, a null xp value was returning a 'blank' api client in the get request
                    return await HandleError(supplierDetails, token);
                }
                return apiClient;
            }
            catch
            {
                // else create and return the new api client
                return await HandleError(supplierDetails, token);
            }

        }
        public async Task<ApiClient> HandleError(Supplier supplierDetails, string token)
        {
            var supplierClient = await _oc.ApiClients.CreateAsync(new ApiClient()
            {
                AppName = $"Integration Client {supplierDetails.Name}",
                Active = true,
                DefaultContextUserName = $"dev_{supplierDetails.ID}",
                ClientSecret = _settings.OrderCloudSettings.MiddlewareClientSecret,
                AccessTokenDuration = 600,
                RefreshTokenDuration = 43200,
                AllowAnyBuyer = false,
                AllowAnySupplier = false,
                AllowSeller = false,
                IsAnonBuyer = false,
            }, token);
            // Assign Supplier API Client to new supplier
            await _oc.ApiClients.SaveAssignmentAsync(new ApiClientAssignment()
            {
                ApiClientID = supplierClient.ID,
                SupplierID = supplierDetails.ID
            }, token);
            // Update supplierXp to contain the new api client value
            await _oc.Suppliers.PatchAsync(supplierDetails.ID, new PartialSupplier { xp = new { ApiClientID = supplierClient.ID } });
            return supplierClient;
        }
    }
}
