using System;
using System.Collections.ObjectModel;
using Microsoft.Azure.Documents;
using Newtonsoft.Json;

namespace OrderCloud.Integrations.Library.Cosmos
{
    public interface ICosmosObject
    {
        string id { get; set; }

        DateTimeOffset timeStamp { get; set; }
    }

    public abstract class CosmosObject : ICosmosObject
    {
        [JsonProperty("id")]
        public string id { get; set; } = Guid.NewGuid().ToString();

        public DateTimeOffset timeStamp { get; set; } = DateTimeOffset.Now;

        // Note, Cosmos unique keys are only unique within the partition.
        public static Collection<UniqueKey> GetUniqueKeys() => null;
    }
}
