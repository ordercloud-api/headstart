using System;
using System.Reflection;
using System.Threading.Tasks;
using Headstart.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ordercloud.integrations.library;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.API.Commands
{
    public interface ISupplierSyncCommand
    {
        Task<JObject> GetOrderAsync(string ID, DecodedToken decodedToken);
    }

    public class SupplierSyncCommand : ISupplierSyncCommand
    {
        private readonly AppSettings _settings;
        private readonly IOrderCloudClient _oc;

        public SupplierSyncCommand(AppSettings settings, IOrderCloudClient oc)
        {
            _settings = settings;
            _oc = oc;
        }

        public async Task<JObject> GetOrderAsync(string ID, DecodedToken decodedToken)
        {
            var supplierID = ID.Split("-")[1];
            var me = await _oc.Me.GetAsync(accessToken: decodedToken.AccessToken);
            Require.That(decodedToken.CommerceRole == CommerceRole.Seller || supplierID == me.Supplier.ID, new ErrorCode("Unauthorized", $"You are not authorized view this order", 401));
            try
            {
                var type = 
                    Assembly.GetExecutingAssembly().GetTypeByAttribute<SupplierSyncAttribute>(attribute => attribute.SupplierID == supplierID) ??
                    Assembly.GetExecutingAssembly().GetTypeByAttribute<SupplierSyncAttribute>(attribute => attribute.SupplierID == "Generic");
                if (type == null) throw new MissingMethodException($"Command for {supplierID} is unavailable");

                var command = (ISupplierSyncCommand) Activator.CreateInstance(type, _settings);
                var method = command.GetType().GetMethod($"GetOrderAsync", BindingFlags.Public | BindingFlags.Instance);
                if (method == null)
                    throw new MissingMethodException($"Get Order Method for {supplierID} is unavailable");

                return await (Task<JObject>) method.Invoke(command, new object[] {ID, decodedToken });
            }
            catch (MissingMethodException mex)
            {
                throw new Exception(JsonConvert.SerializeObject(new ApiError()
                {
                    Data = new { decodedToken, OrderID = ID},
                    ErrorCode = mex.Message,
                    Message = $"Missing Method for: {supplierID ?? "Invalid Supplier"}"
                }));
            }
        }
    }
}
