using Headstart.Models;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Misc;
using System.Linq;
using Headstart.Common;
using System;

namespace Headstart.API.Commands
{
    public interface IHSBuyerCommand
    {
        Task<SuperHSBuyer> Create(SuperHSBuyer buyer);
        Task<SuperHSBuyer> Create(SuperHSBuyer buyer, string accessToken, IOrderCloudClient oc);
        Task<SuperHSBuyer> Get(string buyerID);
        Task<SuperHSBuyer> Update(string buyerID, SuperHSBuyer buyer);
    }
    public class HSBuyerCommand : IHSBuyerCommand
    {
        private readonly IOrderCloudClient _oc;
        private readonly AppSettings _settings;

        public HSBuyerCommand(AppSettings settings, IOrderCloudClient oc)
        {
            _settings = settings;
            _oc = oc;
        }
        public async Task<SuperHSBuyer> Create(SuperHSBuyer superBuyer)
        {
            return await Create(superBuyer, null, _oc);
        }

        public async Task<SuperHSBuyer> Create(SuperHSBuyer superBuyer, string accessToken, IOrderCloudClient oc)
        {
            var createdImpersonationConfig = new ImpersonationConfig();
            var createdBuyer = await CreateBuyerAndRelatedFunctionalResources(superBuyer.Buyer, accessToken, oc);
            var createdMarkup = await CreateMarkup(superBuyer.Markup, createdBuyer.ID, accessToken, oc);
            if(superBuyer?.ImpersonationConfig != null)
            {
                createdImpersonationConfig = await SaveImpersonationConfig(superBuyer.ImpersonationConfig, createdBuyer.ID, accessToken, oc);
            }
            return new SuperHSBuyer()
            {
                Buyer = createdBuyer,
                Markup = createdMarkup,
                ImpersonationConfig = createdImpersonationConfig
            };
        }

        public async Task<SuperHSBuyer> Update(string buyerID, SuperHSBuyer superBuyer)
        {
            // to prevent changing buyerIDs
            superBuyer.Buyer.ID = buyerID;
            var updatedImpersonationConfig = new ImpersonationConfig();

            var updatedBuyer = await _oc.Buyers.SaveAsync<HSBuyer>(buyerID, superBuyer.Buyer);
            var updatedMarkup = await UpdateMarkup(superBuyer.Markup, superBuyer.Buyer.ID);
            if(superBuyer.ImpersonationConfig != null)
            {
                updatedImpersonationConfig = await SaveImpersonationConfig(superBuyer.ImpersonationConfig, buyerID);
            }
            return new SuperHSBuyer()
            {
                Buyer = updatedBuyer,
                Markup = updatedMarkup,
                ImpersonationConfig = updatedImpersonationConfig
            };
        }

        public async Task<SuperHSBuyer> Get(string buyerID)
        {
            var configReq = GetImpersonationByBuyerID(buyerID);
            var buyer = await _oc.Buyers.GetAsync<HSBuyer>(buyerID);
            var config = await configReq;

            // to move into content docs logic
            var markupPercent = buyer.xp?.MarkupPercent ?? 0;
            var markup = new BuyerMarkup()
            {
                Percent = markupPercent
            };

            return new SuperHSBuyer()
            {
                Buyer = buyer,
                Markup = markup,
                ImpersonationConfig = config
            };
        }

        private async Task<ImpersonationConfig> GetImpersonationByBuyerID(string buyerID)
        {
            var config = await _oc.ImpersonationConfigs.ListAsync(filters: $"BuyerID={buyerID}");
            return config?.Items?.FirstOrDefault();
        }

        public async Task<HSBuyer> CreateBuyerAndRelatedFunctionalResources(HSBuyer buyer, string accessToken, IOrderCloudClient oc)
        {
            // if we're seeding then use the passed in oc client
            // to support multiple environments and ease of setup for new orgs
            // else used the configured client
            var token = oc == null ? null : accessToken;
            var ocClient = oc ?? _oc;

            buyer.ID = buyer.ID ?? "{buyerIncrementor}";
            var ocBuyer = await ocClient.Buyers.CreateAsync(buyer, accessToken);
            var ocBuyerID = ocBuyer.ID;
            buyer.ID = ocBuyerID;

            // create base security profile assignment
            await ocClient.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment
            {
                BuyerID = ocBuyerID,
                SecurityProfileID = CustomRole.HSBaseBuyer.ToString()
            }, token);

            // assign message sender
            await ocClient.MessageSenders.SaveAssignmentAsync(new MessageSenderAssignment
            {
                MessageSenderID = "BuyerEmails",
                BuyerID = ocBuyerID
            }, token);

            await ocClient.Incrementors.SaveAsync($"{ocBuyerID}-UserIncrementor", 
                new Incrementor { ID = $"{ocBuyerID}-UserIncrementor", LastNumber = 0, LeftPaddingCount = 5, Name = "User Incrementor" }, token);
            await ocClient.Incrementors.SaveAsync($"{ocBuyerID}-LocationIncrementor",
                new Incrementor { ID = $"{ocBuyerID}-LocationIncrementor", LastNumber = 0, LeftPaddingCount = 4, Name = "Location Incrementor" }, token);

            await ocClient.Catalogs.SaveAssignmentAsync(new CatalogAssignment()
            {
                BuyerID = ocBuyerID,
                CatalogID = ocBuyerID,
                ViewAllCategories = true,
                ViewAllProducts = false
            }, token);
            return buyer;
        }

        private async Task<BuyerMarkup> CreateMarkup(BuyerMarkup markup, string buyerID, string accessToken, IOrderCloudClient oc)
        {
            // if we're seeding then use the passed in oc client
            // to support multiple environments and ease of setup for new orgs
            // else used the configured client
            var token = oc == null ? null : accessToken;
            var ocClient = oc ?? _oc;

            // to move from xp to contentdocs, that logic will go here instead of a patch
            var updatedBuyer = await ocClient.Buyers.PatchAsync(buyerID, new PartialBuyer() { xp = new { MarkupPercent = markup.Percent } }, token);
            return new BuyerMarkup()
            {
                Percent = (int)updatedBuyer.xp.MarkupPercent
            };
        }

        private async Task<ImpersonationConfig> SaveImpersonationConfig(ImpersonationConfig impersonation, string buyerID)
        {
            return await SaveImpersonationConfig(impersonation, buyerID, null, _oc);
        }

        private async Task<ImpersonationConfig> SaveImpersonationConfig(ImpersonationConfig impersonation, string buyerID, string accessToken, IOrderCloudClient oc = null)
        {
            // if we're seeding then use the passed in oc client
            // to support multiple environments and ease of setup for new orgs
            // else used the configured client
            var token = oc == null ? null : accessToken;
            var ocClient = oc ?? _oc;

            var currentConfig = await GetImpersonationByBuyerID(buyerID);
            if(currentConfig != null && impersonation == null)
            {
                await ocClient.ImpersonationConfigs.DeleteAsync(currentConfig.ID);
                return null;
            }
            else if(currentConfig != null)
            {
                return await ocClient.ImpersonationConfigs.SaveAsync(currentConfig.ID, impersonation, token);
            } else
            {
                impersonation.BuyerID = buyerID;
                impersonation.SecurityProfileID = Enum.GetName(typeof(CustomRole), CustomRole.HSBaseBuyer);
                impersonation.ID = $"hs_admin_{buyerID}";
                return await ocClient.ImpersonationConfigs.CreateAsync(impersonation);
            }
        }
        private async Task<BuyerMarkup> UpdateMarkup(BuyerMarkup markup, string buyerID)
        {
            // to move from xp to contentdocs, that logic will go here instead of a patch
            // currently duplicate of the function above, this might need to be duplicated since there wont be a need to save the contentdocs assignment again
            var updatedBuyer = await _oc.Buyers.PatchAsync(buyerID, new PartialBuyer() { xp = new { MarkupPercent = markup.Percent } });
            return new BuyerMarkup()
            {
                Percent = (int)updatedBuyer.xp.MarkupPercent
            };
        }
    }
}
