using Headstart.Common.Services.CMS;
using Headstart.Common.Services.CMS.Models;
using Headstart.Models;
using Microsoft.AspNetCore.Mvc;
using OrderCloud.Catalyst;
using System.Threading.Tasks;

namespace Headstart.API.Controllers
{
	[Route("assets")] 
	public class AssetController : CatalystController
	{
		private readonly IAssetClient _command;

		/// <summary>
		/// The IOC based constructor method for the AssetController class object with Dependency Injection
		/// </summary>
		/// <param name="command"></param>
		public AssetController(IAssetClient command)
		{
			_command = command;
		}

		/// <summary>
		/// Creates an Image action (POST method)
		/// </summary>
		/// <param name="asset"></param>
		/// <returns>The newly created ImageAsset object</returns>
		[HttpPost, Route("image"), OrderCloudUserAuth()]
		public async Task<ImageAsset> CreateImage([FromForm] AssetUpload asset)
		{
			return await _command.CreateImage(asset);
		}

		/// <summary>
		/// Removes/Deletes an Asset action (DELETE method)
		/// </summary>
		/// <param name="id"></param>
		/// <returns></returns>
		[HttpDelete, Route("{id}"), OrderCloudUserAuth()]
		public async Task DeleteImage(string id)
		{
			await _command.DeleteAsset(id);
		}

		/// <summary>
		/// Creates a Document action (POST method)
		/// </summary>
		/// <param name="asset"></param>
		/// <returns>The newly created DocumentAsset object</returns>
		[HttpPost, Route("document"), OrderCloudUserAuth()]
		public async Task<DocumentAsset> CreateDocument([FromForm] AssetUpload asset)
		{
			return await _command.CreateDocument(asset);
		}
	}
}