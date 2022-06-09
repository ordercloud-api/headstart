using System.Threading.Tasks;
using Headstart.Common.Models;
using Newtonsoft.Json.Linq;
using OrderCloud.Catalyst;

namespace Headstart.Common.Commands
{
    public interface ISupplierSyncCommand
    {
        Task<JObject> GetOrderAsync(string id, OrderType orderType, DecodedToken decodedToken);
    }
}
