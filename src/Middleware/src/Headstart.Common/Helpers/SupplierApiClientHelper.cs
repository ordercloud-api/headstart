using System.Threading.Tasks;
using Headstart.Common.Settings;
using OrderCloud.SDK;

namespace Headstart.API.Helpers
{
    public interface ISupplierApiClientHelper
    {
        Task<ApiClient> GetSupplierApiClient(string supplierID, string token);
    }

    public class SupplierApiClientHelper : ISupplierApiClientHelper
    {
        private readonly IOrderCloudClient oc;
        private readonly OrderCloudSettings orderCloudSettings;

        public SupplierApiClientHelper(OrderCloudSettings orderCloudSettings, IOrderCloudClient oc)
        {
            this.orderCloudSettings = orderCloudSettings;
            this.oc = oc;
        }

        public async Task<ApiClient> GetSupplierApiClient(string supplierID, string token)
        {
            Supplier supplierDetails = await oc.Suppliers.GetAsync(supplierID);

            // GET ApiClient using the xp value on supplier
            try
            {
                ApiClient apiClient = await oc.ApiClients.GetAsync(supplierDetails?.xp?.ApiClientID);

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
            var supplierClient = await oc.ApiClients.CreateAsync(
                new ApiClient()
                {
                    AppName = $"Integration Client {supplierDetails.Name}",
                    Active = true,
                    DefaultContextUserName = $"dev_{supplierDetails.ID}",
                    ClientSecret = orderCloudSettings.MiddlewareClientSecret,
                    AccessTokenDuration = 600,
                    RefreshTokenDuration = 43200,
                    AllowAnyBuyer = false,
                    AllowAnySupplier = false,
                    AllowSeller = false,
                    IsAnonBuyer = false,
                },
                token);

            // Assign Supplier API Client to new supplier
            await oc.ApiClients.SaveAssignmentAsync(
                new ApiClientAssignment()
                {
                    ApiClientID = supplierClient.ID,
                    SupplierID = supplierDetails.ID,
                },
                token);

            // Update supplierXp to contain the new api client value
            await oc.Suppliers.PatchAsync(supplierDetails.ID, new PartialSupplier { xp = new { ApiClientID = supplierClient.ID } });
            return supplierClient;
        }
    }
}
