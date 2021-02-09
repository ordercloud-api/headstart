using OrderCloud.SDK;
using System.Threading.Tasks;
using ordercloud.integrations.library;
using Headstart.Common.Models;
using System.Linq;
using Headstart.Common.Repositories;
using Microsoft.Azure.Cosmos;

namespace Headstart.API.Commands
{
    public interface IRMACommand
    {
        Task<RMA> GenerateRMA(RMA rma, VerifiedUserContext verifiedUser);
        Task<CosmosListPage<RMA>> ListMeRMAs(CosmosListOptions listOptions, VerifiedUserContext verifiedUser);
        Task<CosmosListPage<RMA>> ListBuyerRMAs(CosmosListOptions listOptions, VerifiedUserContext verifiedUser);
    }

    public class RMACommand : IRMACommand
    {
        private readonly IRMARepo _rmaRepo;

        public RMACommand(IRMARepo rmaRepo)
        {
            _rmaRepo = rmaRepo;
        }

        public async Task<RMA> GenerateRMA(RMA rma, VerifiedUserContext verifiedUser)
        {
            return await _rmaRepo.AddItemAsync(rma);
        }

        public async Task<CosmosListPage<RMA>> ListMeRMAs(CosmosListOptions listOptions, VerifiedUserContext verifiedUser)
        {
            IQueryable<RMA> queryable = _rmaRepo.GetQueryable().Where(rma => rma.FromBuyerUserID == verifiedUser.UserID);

            CosmosListPage<RMA> rmas = await GenerateRMAList(queryable, listOptions);
            return rmas;
        }

        public async Task<CosmosListPage<RMA>> ListBuyerRMAs(CosmosListOptions listOptions, VerifiedUserContext verifiedUser)
        {
            IQueryable<RMA> queryable = _rmaRepo.GetQueryable().Where(rma => rma.FromBuyerID == verifiedUser.BuyerID);

            CosmosListPage<RMA> rmas = await GenerateRMAList(queryable, listOptions);
            return rmas;
        }

        private async Task<CosmosListPage<RMA>> GenerateRMAList(IQueryable<RMA> queryable, CosmosListOptions listOptions)
        {
            QueryRequestOptions requestOptions = new QueryRequestOptions();
            requestOptions.MaxItemCount = listOptions.PageSize;

            CosmosListPage<RMA> rmas = await _rmaRepo.GetItemsAsync(queryable, requestOptions, listOptions.ContinuationToken);
            return rmas;
        }
    }
}
