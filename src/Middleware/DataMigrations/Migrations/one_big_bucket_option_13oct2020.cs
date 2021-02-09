using Microsoft.Azure.CosmosDB.BulkExecutor.BulkUpdate;
using Newtonsoft.Json;
using ordercloud.integrations.cms;
using ordercloud.integrations.library;
using ordercloud.integrations.library.Cosmos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataMigrations.Migrations
{
	public class one_big_bucket_option_13oct2020
	{
		private readonly ICosmosBulkOperations _bulk;

		public one_big_bucket_option_13oct2020(ICosmosBulkOperations bulk)
		{
			_bulk = bulk;
		}

		private List<string> ContainersToDelete = new List<string>()
		{
			"129f60a7-d75c-423e-b5a6-caa91ca27441",
			"1c3fdae4-d364-48fe-82fc-4cceaefed56b",
			"753cb11e-0e09-47ce-97ba-b970f6ac8879",
			"9c500eca-847b-40ee-8024-2aaa96f879ce",
			"e495c4d5-f3e9-4b0c-b341-1b265e4ce427",
			"baddc784-49e5-4b96-8d4c-b185d2ba2101",
			"2a22eada-ded5-46cb-8fc0-bdb59f32d19d",
			"1c10d559-b91c-47cd-b5d5-74332af0b6fd",
			"0c4b7852-dd7b-44f7-b06f-8aeba3f95061",
			"c5456062-4cbe-48a6-86a1-782fbb379a2d",
			"cb9e8609-a4f5-4281-adac-23f367b4b91a",
			"40e5dde2-ce47-4b7b-a3e8-4c8a81d7a72b",
			"e31b7875-60eb-4a2e-898a-4e7e5557da11",
			"7981edbd-7aa8-47cd-95ba-7b787002c114",
			"524cefcd-b7c0-44e1-877f-1786fd88b8fd",
			"4263b97e-d6c2-42a9-87e7-82fe3eed4af9",
			"dd57af2f-f1fe-4ab3-8498-29c90482881b",
			"70c50637-2e3f-4e59-a612-e9ef082a74d5",
			"6195e37d-c66a-4c19-bdc3-608ea88df405",
			"cbfbb2f9-0b34-4e33-b5c2-da4d6554d6c6"
		};

		// This run merges multiple containers into 1 for the winmark organizations.
		public async Task Run()
		{
			var assets = await _bulk.GetAllAsync<AssetDO>("assets");
			var toDelete = assets
				.Where(asset => ContainersToDelete.Contains(asset.ContainerID)).ToList();
			var toImport = toDelete
				.Select(asset =>
				{
					var clone = JsonConvert.DeserializeObject<AssetDO>(JsonConvert.SerializeObject(asset));
					clone.ContainerID = "0c0ffc82-bf34-4850-b1e3-e307f0993721";
				return clone;
			}).ToList();


			await _bulk.ImportAsync<AssetDO>("assets", toImport);
			//await _bulk.Delete<AssetDO>("assets", toDelete);
		}
	}
}
