using Microsoft.Azure.CosmosDB.BulkExecutor.BulkUpdate;
using Microsoft.Azure.CosmosDB.BulkExecutor.BulkDelete;
using Newtonsoft.Json.Linq;
using NPOI.HSSF.Record.PivotTable;
using NPOI.SS.Formula.Functions;
using ordercloud.integrations.cms;
using ordercloud.integrations.library;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMigrations.Migrations
{
	public class new_asset_types_8oct2020
	{
		private readonly ICosmosBulkOperations _bulk;

		public new_asset_types_8oct2020(ICosmosBulkOperations bulk) 
		{
			_bulk = bulk;
		}

		// This run removes all the existing asset types except images and replaces them with types derived from the ContentType.
		// The old Attachment type is now represented with a specific title, which can be queried on.
		public async Task Run()
		{
			await _bulk.UpdateAllAsync<AssetDO>("assets", asset =>
			{
				var updates = new List<UpdateOperation>();

				var typeToken = asset.SelectToken("Type");
				var type = (typeToken.Type != JTokenType.Null) ? typeToken?.ToObject<AssetType>() : null;
				var contentType = asset.SelectToken("Metadata.ContentType")?.ToObject<string>();

				if (type != AssetType.Image)
				{
					var newType = AssetMapper.DetectAssetTypeFromContentType(contentType);
					var updateType = new SetUpdateOperation<AssetType?>("Type", newType);
					updates.Add(updateType);

					if (type?.To<int>() == 2) // old attachment type
					{
						var updateTitle = new SetUpdateOperation<string>("Title", "Product_Attachment");
						updates.Add(updateTitle);
					}
				}
				return updates;
			});
			await _bulk.UpdateAllAsync<AssetedResourceDO>("assetedresource", assignment =>
			{
				var allOthers = assignment.SelectToken("AllOtherAssetIDs")?.ToObject<List<string>>();

				if (allOthers != null) return new List<UpdateOperation>();

				var themes = assignment.SelectToken("ThemeAssetIDs").ToObject<List<string>>();
				var attachments = assignment.SelectToken("AttachmentAssetIDs").ToObject<List<string>>();
				var structured = assignment.SelectToken("StructuredAssetsIDs").ToObject<List<string>>();

				var array = themes.Concat(attachments).Concat(structured).ToList();
				return new List<UpdateOperation>()
				{
					new SetUpdateOperation<List<string>>("AllOtherAssetIDs", array)
				};
			});
		}
	}
}
