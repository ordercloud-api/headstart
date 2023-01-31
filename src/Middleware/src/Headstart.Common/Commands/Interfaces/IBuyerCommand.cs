using System.Threading.Tasks;
using Headstart.Common.Models;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
{
    public interface IBuyerCommand
    {
        Task<SuperHSBuyer> Create(SuperHSBuyer buyer);

        Task<SuperHSBuyer> Create(SuperHSBuyer buyer, string accessToken, IOrderCloudClient oc);

        Task<SuperHSBuyer> Get(string buyerID);

        Task<SuperHSBuyer> Save(string buyerID, SuperHSBuyer buyer);
    }
}
