using System.Threading.Tasks;
using Headstart.Common.Models;
using OrderCloud.Catalyst;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
{
    public interface ISupplierCommand
    {
        Task<HSSupplier> Create(HSSupplier supplier, string accessToken, bool isSeedingEnvironment = false);

        Task<HSSupplier> GetMySupplier(string supplierID, DecodedToken decodedToken);

        Task<HSSupplier> Patch(string supplierID, PartialSupplier supplier, DecodedToken decodedToken);

        Task<HSSupplierOrderData> GetSupplierOrderData(string supplierOrderID, OrderType orderType, DecodedToken decodedToken);
    }
}
