using System;
using System.Linq;
using System.Threading.Tasks;
using Headstart.Common.Constants;
using Headstart.Common.Models;
using OrderCloud.SDK;

namespace Headstart.Common.Commands
{
    public class BuyerCommand : IBuyerCommand
    {
        private readonly IOrderCloudClient oc;

        public BuyerCommand(IOrderCloudClient oc)
        {
            this.oc = oc;
        }

        public async Task<SuperHSBuyer> Create(SuperHSBuyer superBuyer)
        {
            return await Create(superBuyer, null, oc);
        }

        public async Task<SuperHSBuyer> Create(SuperHSBuyer superBuyer, string accessToken, IOrderCloudClient oc)
        {
            var createdImpersonationConfig = new ImpersonationConfig();
            var createdBuyer = await CreateBuyerAndRelatedFunctionalResources(superBuyer.Buyer, accessToken, oc);
            if (superBuyer?.ImpersonationConfig != null)
            {
                createdImpersonationConfig = await SaveImpersonationConfig(superBuyer.ImpersonationConfig, createdBuyer.ID, accessToken, oc);
            }

            return new SuperHSBuyer()
            {
                Buyer = createdBuyer,
                ImpersonationConfig = createdImpersonationConfig,
            };
        }

        public async Task<SuperHSBuyer> Save(string buyerID, SuperHSBuyer superBuyer)
        {
            // to prevent changing buyerIDs
            superBuyer.Buyer.ID = buyerID;
            ImpersonationConfig updatedImpersonationConfig = null;

            var updatedBuyer = await oc.Buyers.SaveAsync<HSBuyer>(buyerID, superBuyer.Buyer);
            if (superBuyer.ImpersonationConfig != null)
            {
                updatedImpersonationConfig = await SaveImpersonationConfig(superBuyer.ImpersonationConfig, buyerID);
            }

            return new SuperHSBuyer()
            {
                Buyer = updatedBuyer,
                ImpersonationConfig = updatedImpersonationConfig,
            };
        }

        public async Task<SuperHSBuyer> Get(string buyerID)
        {
            var configReq = GetImpersonationByBuyerID(buyerID);
            var buyer = await oc.Buyers.GetAsync<HSBuyer>(buyerID);
            var config = await configReq;

            return new SuperHSBuyer()
            {
                Buyer = buyer,
                ImpersonationConfig = config,
            };
        }

        public async Task<HSBuyer> CreateBuyerAndRelatedFunctionalResources(HSBuyer buyer, string accessToken, IOrderCloudClient oc)
        {
            // if we're seeding then use the passed in oc client
            // to support multiple environments and ease of setup for new orgs
            // else used the configured client
            var token = oc == null ? null : accessToken;
            var ocClient = oc ?? this.oc;

            buyer.ID = buyer.ID ?? "{buyerIncrementor}";
            var ocBuyer = await ocClient.Buyers.CreateAsync(buyer, accessToken);
            var ocBuyerID = ocBuyer.ID;
            buyer.ID = ocBuyerID;

            // create base security profile assignment
            await ocClient.SecurityProfiles.SaveAssignmentAsync(
                new SecurityProfileAssignment
                {
                    BuyerID = ocBuyerID,
                    SecurityProfileID = CustomRole.HSBaseBuyer.ToString(),
                }, token);

            // assign message sender
            await ocClient.MessageSenders.SaveAssignmentAsync(
                new MessageSenderAssignment
                {
                    MessageSenderID = MessageSenderConstants.BuyerEmails,
                    BuyerID = ocBuyerID,
                }, token);

            await ocClient.Incrementors.SaveAsync(
                $"{ocBuyerID}-UserIncrementor",
                new Incrementor { ID = $"{ocBuyerID}-UserIncrementor", LastNumber = 0, LeftPaddingCount = 5, Name = "User Incrementor" },
                token);
            await ocClient.Incrementors.SaveAsync(
                $"{ocBuyerID}-LocationIncrementor",
                new Incrementor { ID = $"{ocBuyerID}-LocationIncrementor", LastNumber = 0, LeftPaddingCount = 4, Name = "Location Incrementor" },
                token);

            await ocClient.Catalogs.SaveAssignmentAsync(
                new CatalogAssignment()
                {
                    BuyerID = ocBuyerID,
                    CatalogID = ocBuyerID,
                    ViewAllCategories = true,
                    ViewAllProducts = false,
                }, token);
            return buyer;
        }

        private async Task<ImpersonationConfig> GetImpersonationByBuyerID(string buyerID)
        {
            var config = await oc.ImpersonationConfigs.ListAsync(filters: $"BuyerID={buyerID}");
            return config?.Items?.FirstOrDefault();
        }

        private async Task<ImpersonationConfig> SaveImpersonationConfig(ImpersonationConfig impersonation, string buyerID)
        {
            return await SaveImpersonationConfig(impersonation, buyerID, null, oc);
        }

        private async Task<ImpersonationConfig> SaveImpersonationConfig(ImpersonationConfig impersonation, string buyerID, string accessToken, IOrderCloudClient oc = null)
        {
            // if we're seeding then use the passed in oc client
            // to support multiple environments and ease of setup for new orgs
            // else used the configured client
            var token = oc == null ? null : accessToken;
            var ocClient = oc ?? this.oc;

            var currentConfig = await GetImpersonationByBuyerID(buyerID);
            if (currentConfig != null && impersonation == null)
            {
                await ocClient.ImpersonationConfigs.DeleteAsync(currentConfig.ID);
                return null;
            }
            else if (currentConfig != null)
            {
                return await ocClient.ImpersonationConfigs.SaveAsync(currentConfig.ID, impersonation, token);
            }
            else
            {
                impersonation.BuyerID = buyerID;
                impersonation.SecurityProfileID = Enum.GetName(typeof(CustomRole), CustomRole.HSBaseBuyer);
                impersonation.ID = $"hs_admin_{buyerID}";
                return await ocClient.ImpersonationConfigs.CreateAsync(impersonation);
            }
        }
    }
}
