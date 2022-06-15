using System.Threading.Tasks;
using Headstart.Common.Models;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
{
    public interface IHSBuyerLocationCommand
    {
        Task<HSBuyerLocation> Get(string buyerID, string buyerLocationID);

        Task<HSBuyerLocation> Create(string buyerID, HSBuyerLocation buyerLocation);

        Task<HSBuyerLocation> Save(string buyerID, string buyerLocationID, HSBuyerLocation buyerLocation);

        Task<HSBuyerLocation> Save(string buyerID, string buyerLocationID, HSBuyerLocation buyerLocation, string token, IOrderCloudClient oc = null);

        Task Delete(string buyerID, string buyerLocationID);

        Task CreateSinglePermissionGroup(string buyerLocationID, string permissionGroupID);

        Task ReassignUserGroups(string buyerID, string newUserID);
    }
}
