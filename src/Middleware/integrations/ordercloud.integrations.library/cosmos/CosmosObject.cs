using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reflection;
using Cosmonaut.Attributes;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace ordercloud.integrations.library
{
	public interface ICosmosObject
	{
		string id { get; set; }
		DateTimeOffset timeStamp { get; set; }
	}

	public abstract class CosmosObject : ICosmosObject
	{
		[ApiIgnore]
		[JsonProperty("id")]
		public string id { get; set; } = Guid.NewGuid().ToString();
		[ApiIgnore]
		public DateTimeOffset timeStamp { get; set; } = DateTimeOffset.Now;
		// Note, Cosmos unique keys are only unique within the partition.
		public static Collection<UniqueKey> GetUniqueKeys() => null;
	}
}
