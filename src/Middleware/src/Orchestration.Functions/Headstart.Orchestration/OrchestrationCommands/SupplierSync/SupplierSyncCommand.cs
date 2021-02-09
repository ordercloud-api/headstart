using System;
using System.Reflection;
using System.Threading.Tasks;
using Headstart.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ordercloud.integrations.library;
using OrderCloud.SDK;

namespace Headstart.Orchestration
{
    public interface ISupplierSyncCommand
    {
        Task<JObject> GetOrderAsync(string ID, VerifiedUserContext user);
    }

    public class SupplierSyncCommand : ISupplierSyncCommand
    {
        private readonly AppSettings _settings;

        public SupplierSyncCommand(AppSettings settings)
        {
            _settings = settings;
        }

        public async Task<JObject> GetOrderAsync(string ID, VerifiedUserContext user)
        {
            var supplierID = ID.Split("-")[1];
            Require.That(user.UsrType == "admin" || supplierID == user.SupplierID, new ErrorCode("Unauthorized", 401, $"You are not authorized view this order"));
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

                return await (Task<JObject>) method.Invoke(command, new object[] {ID, user});
            }
            catch (MissingMethodException mex)
            {
                throw new Exception(JsonConvert.SerializeObject(new ApiError()
                {
                    Data = new {user, OrderID = ID},
                    ErrorCode = mex.Message,
                    Message = $"Missing Method for: {supplierID ?? "Invalid Supplier"}"
                }));
            }
        }
    }
}
