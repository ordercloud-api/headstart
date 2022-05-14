using Headstart.Models;
using OrderCloud.SDK;
using System.Threading.Tasks;
using Headstart.Models.Misc;
using System.Linq;
using Headstart.Common;
using System;
using Sitecore.Foundation.SitecoreExtensions.Extensions;
using SitecoreExtensions = Sitecore.Foundation.SitecoreExtensions.Extensions;

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
		private readonly IOrderCloudClient oc;
		private readonly AppSettings settings; 

		/// <summary>
		/// The IOC based constructor method for the HSBuyerCommand class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="oc"></param>
		public HSBuyerCommand(AppSettings settings, IOrderCloudClient oc)
		{
			try
			{
				this.settings = settings;
				this.oc = oc;
			}
			catch (Exception ex)
			{
				LoggingNotifications.LogApiResponseMessages(this.settings.LogSettings, SitecoreExtensions.Helpers.GetMethodName(), "",
					LoggingNotifications.GetExceptionMessagePrefixKey(), true, ex.Message, ex.StackTrace, ex);
			}
		}

		/// <summary>
		/// Public re-usable Create task method for creating a SuperHsBuyer
		/// </summary>
		/// <param name="superBuyer"></param>
		/// <returns>The newly created SuperHSBuyer object</returns>
		public async Task<SuperHSBuyer> Create(SuperHSBuyer superBuyer)
		{
            return await Create(superBuyer, null, oc);
		}

		/// <summary>
		/// Public re-usable Create task method for creating a SuperHsBuyer
		/// </summary>
		/// <param name="superBuyer"></param>
		/// <param name="accessToken"></param>
		/// <param name="oc"></param>
		/// <returns>The newly created SuperHSBuyer object</returns>
		public async Task<SuperHSBuyer> Create(SuperHSBuyer superBuyer, string accessToken, IOrderCloudClient oc)
		{
			var createdImpersonationConfig = new ImpersonationConfig();
			var createdBuyer = await CreateBuyerAndRelatedFunctionalResources(superBuyer.Buyer, accessToken, oc);
			var createdMarkup = await CreateMarkup(superBuyer.Markup, createdBuyer.ID, accessToken, oc);
			if (superBuyer?.ImpersonationConfig != null)
			{
				createdImpersonationConfig = await SaveImpersonationConfig(superBuyer.ImpersonationConfig, createdBuyer.ID, accessToken, oc);
			}
			return new SuperHSBuyer()
			{
				Buyer = createdBuyer,
				Markup = createdMarkup,
                ImpersonationConfig = createdImpersonationConfig,
			};
		}

		/// <summary>
		/// Public re-usable Update task method for updating a SuperHsBuyer
		/// </summary>
		/// <param name="buyerID"></param>
		/// <param name="superBuyer"></param>
		/// <returns>The newly updated SuperHSBuyer object</returns>
		public async Task<SuperHSBuyer> Update(string buyerID, SuperHSBuyer superBuyer)
		{
			// to prevent changing buyerIDs
			superBuyer.Buyer.ID = buyerID;
			var updatedImpersonationConfig = new ImpersonationConfig();

            var updatedBuyer = await oc.Buyers.SaveAsync<HSBuyer>(buyerID, superBuyer.Buyer);
			var updatedMarkup = await UpdateMarkup(superBuyer.Markup, superBuyer.Buyer.ID);
			if (superBuyer.ImpersonationConfig != null)
			{
				updatedImpersonationConfig = await SaveImpersonationConfig(superBuyer.ImpersonationConfig, buyerID);
			}
			return new SuperHSBuyer()
			{
				Buyer = updatedBuyer,
				Markup = updatedMarkup,
                ImpersonationConfig = updatedImpersonationConfig,
			};
		}

		/// <summary>
		/// Public re-usable task method to get the SuperHsBuyer object by the buyerID
		/// </summary>
		/// <param name="buyerID"></param>
		/// <returns>The SuperHSBuyer object by the buyerID</returns>
		public async Task<SuperHSBuyer> Get(string buyerID)
		{
			var configReq = GetImpersonationByBuyerID(buyerID);
            var buyer = await oc.Buyers.GetAsync<HSBuyer>(buyerID);
			var config = await configReq;
			// to move into content docs logic
			var markupPercent = buyer.xp?.MarkupPercent ?? 0;
			var markup = new BuyerMarkup()
			{
                Percent = markupPercent,
			};

			return new SuperHSBuyer()
			{
				Buyer = buyer,
				Markup = markup,
                ImpersonationConfig = config,
			};
		}

		/// <summary>
		/// Public re-usable CreateBuyerAndRelatedFunctionalResources task method to create the HsBuyer object
		/// </summary>
		/// <param name="buyer"></param>
		/// <param name="accessToken"></param>
		/// <param name="oc"></param>
		/// <returns>The newly created HSBuyer object</returns>
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
				MessageSenderID = "BuyerEmails",
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
		
		/// <summary>
		/// Private re-usable GetImpersonationByBuyerID task method to get the ImpersonationConfig object by the buyerID
		/// </summary>
		/// <param name="buyerID"></param>
		/// <returns>The ImpersonationConfig object by the buyerID</returns>
		private async Task<ImpersonationConfig> GetImpersonationByBuyerID(string buyerID)
		{
            var config = await oc.ImpersonationConfigs.ListAsync(filters: $"BuyerID={buyerID}");
			return config?.Items?.FirstOrDefault();
		}

		/// <summary>
		/// Private re-usable CreateMarkup task method to create the BuyerMarkup object
		/// </summary>
		/// <param name="markup"></param>
		/// <param name="buyerID"></param>
		/// <param name="accessToken"></param>
		/// <param name="oc"></param>
		/// <returns>The newly created BuyerMarkup object</returns>
		private async Task<BuyerMarkup> CreateMarkup(BuyerMarkup markup, string buyerID, string accessToken, IOrderCloudClient oc)
		{
			// if we're seeding then use the passed in oc client
			// to support multiple environments and ease of setup for new orgs
			// else used the configured client
			var token = oc == null ? null : accessToken;
            var ocClient = oc ?? this.oc;

			// to move from xp to content docs, that logic will go here instead of a patch
			var updatedBuyer = await ocClient.Buyers.PatchAsync(buyerID, new PartialBuyer() { xp = new { MarkupPercent = markup.Percent } }, token);
			return new BuyerMarkup()
			{
                Percent = (int)updatedBuyer.xp.MarkupPercent,
			};
		}

		/// <summary>
		/// Private re-usable SaveImpersonationConfig task method to update the ImpersonationConfig object data
		/// </summary>
		/// <param name="impersonation"></param>
		/// <param name="buyerID"></param>
		/// <returns>The updated ImpersonationConfig object</returns>
		private async Task<ImpersonationConfig> SaveImpersonationConfig(ImpersonationConfig impersonation, string buyerID)
		{
            return await SaveImpersonationConfig(impersonation, buyerID, null, oc);
		}

		/// <summary>
		/// Private re-usable SaveImpersonationConfig task method to update the ImpersonationConfig object data
		/// </summary>
		/// <param name="impersonation"></param>
		/// <param name="buyerID"></param>
		/// <param name="accessToken"></param>
		/// <param name="oc"></param>
		/// <returns>The updated ImpersonationConfig object</returns>
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

		/// <summary>
		/// Private re-usable UpdateMarkup task method to update the BuyerMarkup object
		/// </summary>
		/// <param name="markup"></param>
		/// <param name="buyerID"></param>
		/// <returns>The newly updated BuyerMarkup object</returns>
		private async Task<BuyerMarkup> UpdateMarkup(BuyerMarkup markup, string buyerID)
		{
			// to move from xp to contentdocs, that logic will go here instead of a patch
			// currently duplicate of the function above, this might need to be duplicated since there wont be a need to save the contentdocs assignment again
            var updatedBuyer = await oc.Buyers.PatchAsync(buyerID, new PartialBuyer() { xp = new { MarkupPercent = markup.Percent } });
			return new BuyerMarkup()
			{
                Percent = (int)updatedBuyer.xp.MarkupPercent,
			};
		}
	}
}
