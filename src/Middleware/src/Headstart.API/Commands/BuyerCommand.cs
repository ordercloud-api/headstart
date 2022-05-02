using System;
using System.Linq;
using OrderCloud.SDK;
using Headstart.Common;
using Sitecore.Diagnostics;
using System.Threading.Tasks;
using Headstart.Common.Models.Misc;
using Headstart.Common.Models.Headstart;
using Sitecore.Foundation.SitecoreExtensions.Extensions;

namespace Headstart.API.Commands
{
	public interface IHsBuyerCommand
	{
		Task<SuperHsBuyer> Create(SuperHsBuyer buyer);
		Task<SuperHsBuyer> Create(SuperHsBuyer buyer, string accessToken, IOrderCloudClient oc);
		Task<SuperHsBuyer> Get(string buyerId);
		Task<SuperHsBuyer> Update(string buyerId, SuperHsBuyer buyer);
	}

	public class HsBuyerCommand : IHsBuyerCommand
	{
		private readonly IOrderCloudClient _oc;
		private readonly AppSettings _settings; 

		/// <summary>
		/// The IOC based constructor method for the HSBuyerCommand class object with Dependency Injection
		/// </summary>
		/// <param name="settings"></param>
		/// <param name="oc"></param>
		public HsBuyerCommand(AppSettings settings, IOrderCloudClient oc)
		{
			try
			{
				_settings = settings;
				_oc = oc;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
		}

		/// <summary>
		/// Public re-usable Create task method for creating a SuperHsBuyer
		/// </summary>
		/// <param name="superBuyer"></param>
		/// <returns>The newly created SuperHsBuyer object</returns>
		public async Task<SuperHsBuyer> Create(SuperHsBuyer superBuyer)
		{
			var resp = new SuperHsBuyer();
			try
			{
				resp = await Create(superBuyer, null, _oc);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable Create task method for creating a SuperHsBuyer
		/// </summary>
		/// <param name="superBuyer"></param>
		/// <param name="accessToken"></param>
		/// <param name="oc"></param>
		/// <returns>The newly created SuperHsBuyer object</returns>
		public async Task<SuperHsBuyer> Create(SuperHsBuyer superBuyer, string accessToken, IOrderCloudClient oc)
		{
			var resp = new SuperHsBuyer();
			try
			{
				var createdImpersonationConfig = new ImpersonationConfig();
				var createdBuyer = await CreateBuyerAndRelatedFunctionalResources(superBuyer.Buyer, accessToken, oc);
				var createdMarkup = await CreateMarkup(superBuyer.Markup, createdBuyer.ID, accessToken, oc);
				if (superBuyer?.ImpersonationConfig != null)
				{
					createdImpersonationConfig = await SaveImpersonationConfig(superBuyer.ImpersonationConfig, createdBuyer.ID, accessToken, oc);
				}
				return new SuperHsBuyer()
				{
					Buyer = createdBuyer,
					Markup = createdMarkup,
					ImpersonationConfig = createdImpersonationConfig
				};
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable Update task method for updating a SuperHsBuyer
		/// </summary>
		/// <param name="buyerId"></param>
		/// <param name="superBuyer"></param>
		/// <returns>The newly updated SuperHsBuyer object</returns>
		public async Task<SuperHsBuyer> Update(string buyerId, SuperHsBuyer superBuyer)
		{
			var resp = new SuperHsBuyer();
			try
			{
				// to prevent changing buyerIds
				superBuyer.Buyer.Id = buyerId;
				var updatedImpersonationConfig = new ImpersonationConfig();

				var updatedBuyer = await _oc.Buyers.SaveAsync<HsBuyer>(buyerId, superBuyer.Buyer);
				var updatedMarkup = await UpdateMarkup(superBuyer.Markup, superBuyer.Buyer.ID);
				if (superBuyer.ImpersonationConfig != null)
				{
					updatedImpersonationConfig = await SaveImpersonationConfig(superBuyer.ImpersonationConfig, buyerId);
				}
				return new SuperHsBuyer()
				{
					Buyer = updatedBuyer,
					Markup = updatedMarkup,
					ImpersonationConfig = updatedImpersonationConfig
				};
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable task method to get the SuperHsBuyer object by the buyerID
		/// </summary>
		/// <param name="buyerId"></param>
		/// <returns>The SuperHsBuyer response object by the buyerId</returns>
		public async Task<SuperHsBuyer> Get(string buyerId)
		{
			var resp = new SuperHsBuyer();
			try
			{
				var configReq = GetImpersonationByBuyerId(buyerId);
				var buyer = await _oc.Buyers.GetAsync<HsBuyer>(buyerId);
				var config = await configReq;
				// To move into content docs logic
				var markupPercent = buyer.xp?.MarkupPercent ?? 0;
				var markup = new BuyerMarkup()
				{
					Percent = markupPercent
				};

				return new SuperHsBuyer()
				{
					Buyer = buyer,
					Markup = markup,
					ImpersonationConfig = config
				};
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable GetImpersonationByBuyerID task method to get the ImpersonationConfig object by the buyerID
		/// </summary>
		/// <param name="buyerId"></param>
		/// <returns>The ImpersonationConfig response object by the buyerId</returns>
		private async Task<ImpersonationConfig> GetImpersonationByBuyerId(string buyerId)
		{
			var resp = new ImpersonationConfig();
			try
			{
				var config = await _oc.ImpersonationConfigs.ListAsync(filters: $"BuyerID={buyerId}");
				resp = config?.Items?.FirstOrDefault();
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Public re-usable CreateBuyerAndRelatedFunctionalResources task method to create the HsBuyer object
		/// </summary>
		/// <param name="buyer"></param>
		/// <param name="accessToken"></param>
		/// <param name="oc"></param>
		/// <returns>The newly created HsBuyer object</returns>
		public async Task<HsBuyer> CreateBuyerAndRelatedFunctionalResources(HsBuyer buyer, string accessToken, IOrderCloudClient oc)
		{
			try
			{
				// if we're seeding then use the passed in oc client
				// to support multiple environments and ease of setup for new orgs
				// else used the configured client
				var token = oc == null ? null : accessToken;
				var ocClient = oc ?? _oc;

				buyer.ID = buyer.ID ?? "{buyerIncrementor}";
				var ocBuyer = await ocClient.Buyers.CreateAsync(buyer, accessToken);
				var ocBuyerId = ocBuyer.ID;
				buyer.ID = ocBuyerId;

				// create base security profile assignment
				await ocClient.SecurityProfiles.SaveAssignmentAsync(new SecurityProfileAssignment
				{
					BuyerID = ocBuyerId,
					SecurityProfileID = CustomRole.HSBaseBuyer.ToString()
				}, token);

				// assign message sender
				await ocClient.MessageSenders.SaveAssignmentAsync(new MessageSenderAssignment
				{
					MessageSenderID = "BuyerEmails",
					BuyerID = ocBuyerId
				}, token);

				await ocClient.Incrementors.SaveAsync($"{ocBuyerId}-UserIncrementor",
					new Incrementor { ID = $"{ocBuyerId}-UserIncrementor", LastNumber = 0, LeftPaddingCount = 5, Name = "User Incrementor" }, token);
				await ocClient.Incrementors.SaveAsync($"{ocBuyerId}-LocationIncrementor",
					new Incrementor { ID = $"{ocBuyerId}-LocationIncrementor", LastNumber = 0, LeftPaddingCount = 4, Name = "Location Incrementor" }, token);

				await ocClient.Catalogs.SaveAssignmentAsync(new CatalogAssignment()
				{
					BuyerID = ocBuyerId,
					CatalogID = ocBuyerId,
					ViewAllCategories = true,
					ViewAllProducts = false
				}, token);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}            
			return buyer;
		}

		/// <summary>
		/// Private re-usable CreateMarkup task method to create the BuyerMarkup object
		/// </summary>
		/// <param name="markup"></param>
		/// <param name="buyerId"></param>
		/// <param name="accessToken"></param>
		/// <param name="oc"></param>
		/// <returns>The newly created BuyerMarkup object</returns>
		private async Task<BuyerMarkup> CreateMarkup(BuyerMarkup markup, string buyerId, string accessToken, IOrderCloudClient oc)
		{
			var resp = new BuyerMarkup();
			try
			{
				// if we're seeding then use the passed in oc client
				// to support multiple environments and ease of setup for new orgs
				// else used the configured client
				var token = oc == null ? null : accessToken;
				var ocClient = oc ?? _oc;

				// to move from xp to content docs, that logic will go here instead of a patch
				var updatedBuyer = await ocClient.Buyers.PatchAsync(buyerId, new PartialBuyer() { xp = new { MarkupPercent = markup.Percent } }, token);
				resp.Percent = (int)updatedBuyer.xp.MarkupPercent;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable SaveImpersonationConfig task method to update the ImpersonationConfig object data
		/// </summary>
		/// <param name="impersonation"></param>
		/// <param name="buyerId"></param>
		/// <returns>The updated ImpersonationConfig object</returns>
		private async Task<ImpersonationConfig> SaveImpersonationConfig(ImpersonationConfig impersonation, string buyerId)
		{
			var resp = new ImpersonationConfig();
			try
			{
				resp = await SaveImpersonationConfig(impersonation, buyerId, null, _oc);
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable SaveImpersonationConfig task method to update the ImpersonationConfig object data
		/// </summary>
		/// <param name="impersonation"></param>
		/// <param name="buyerId"></param>
		/// <param name="accessToken"></param>
		/// <param name="oc"></param>
		/// <returns>The updated ImpersonationConfig object</returns>
		private async Task<ImpersonationConfig> SaveImpersonationConfig(ImpersonationConfig impersonation, string buyerId, string accessToken, IOrderCloudClient oc = null)
		{
			var resp = new ImpersonationConfig();
			try
			{
				// if we're seeding then use the passed in oc client
				// to support multiple environments and ease of setup for new orgs
				// else used the configured client
				var token = oc == null ? null : accessToken;
				var ocClient = oc ?? _oc;

				var currentConfig = await GetImpersonationByBuyerId(buyerId);
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
					impersonation.BuyerID = buyerId;
					impersonation.SecurityProfileID = Enum.GetName(typeof(CustomRole), CustomRole.HSBaseBuyer);
					impersonation.ID = $"hs_admin_{buyerId}";
					return await ocClient.ImpersonationConfigs.CreateAsync(impersonation);
				}
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}
			return resp;
		}

		/// <summary>
		/// Private re-usable UpdateMarkup task method to update the BuyerMarkup object
		/// </summary>
		/// <param name="markup"></param>
		/// <param name="buyerId"></param>
		/// <returns>The newly updated BuyerMarkup object</returns>
		private async Task<BuyerMarkup> UpdateMarkup(BuyerMarkup markup, string buyerId)
		{
			var resp = new BuyerMarkup();
			try
			{
				// to move from xp to contentdocs, that logic will go here instead of a patch
				// currently duplicate of the function above, this might need to be duplicated since there wont be a need to save the contentdocs assignment again
				var updatedBuyer = await _oc.Buyers.PatchAsync(buyerId, new PartialBuyer() { xp = new { MarkupPercent = markup.Percent } });
				resp.Percent = (int)updatedBuyer.xp.MarkupPercent;
			}
			catch (Exception ex)
			{
				LogExt.LogException(_settings.LogSettings, Helpers.GetMethodName(), $@"{LoggingNotifications.GetGeneralLogMessagePrefixKey()}", ex.Message, ex.StackTrace, this, true);
			}            
			return resp;
		}
	}
}