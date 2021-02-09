using System;
using Headstart.Common.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;

namespace Headstart.Common.Exceptions
{
    public class OrchestrationException : Exception
    {
        public OrchestrationError Error { get; set; }

        public OrchestrationException(OrchestrationErrorType type, object data)
        {
            Error = new OrchestrationError
            {
                Type = type,
                Data = data
            };
        }

        public OrchestrationException(OrchestrationErrorType type, string message)
        {
            Error = new OrchestrationError
            {
                Type = type,
                Data = message
            };
        }

        public OrchestrationException(OrchestrationErrorType type, WorkItem wi, object data = null)
        {
            Error = new OrchestrationError()
            {
                Type = type,
                RecordId = wi.RecordId,
                SupplierId = wi.ResourceId,
                Action = wi.Action,
                RecordType = wi.RecordType,
                Current = wi.Current,
                Cache = wi.Cache,
                Diff = wi.Diff,
                Data = data
            };
        }
    }

    public class OrchestrationError
    {
        public OrchestrationError()
        {
           
        }
        [JsonConverter(typeof(StringEnumConverter))]
        public OrchestrationErrorType Type { get; set; }
        public string RecordId { get; set; }
        public string SupplierId { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public RecordType RecordType { get; set; }
        [JsonConverter(typeof(StringEnumConverter))]
        public Models.Action Action { get; set; }
        public JObject Current { get; set; }
        public JObject Cache { get; set; }
        public JToken Diff { get; set; }
        public object Data { get; set; }
    }

    [JsonConverter(typeof(StringEnumConverter))]
    public enum OrchestrationErrorType
    {
        WorkItemDefinition,
        QueuedGetError,
        CachedGetError,
        DiffCalculationError,
        ActionEvaluationError,
        CacheUpdateError,
        QueueCleanupError,
        SyncCommandError,
        CreateExistsError,
        CreateGeneralError,
        UpdateGeneralError,
        PatchGeneralError,
        GetGeneralError,
        AuthenticateSupplierError,
        NoRelatedOrderCloudOrderFound,
    }
}
