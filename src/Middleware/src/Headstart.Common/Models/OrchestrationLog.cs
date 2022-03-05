using Cosmonaut.Attributes;
using Headstart.Common.Exceptions;
using Microsoft.Azure.WebJobs;
using System;
using OrderCloud.SDK;
using Newtonsoft.Json.Linq;
using ordercloud.integrations.library;
using ordercloud.integrations.library.helpers;

namespace Headstart.Common.Models
{
    [CosmosCollection("orchestrationlogs")]
    public class OrchestrationLog : CosmosObject
    {
        public OrchestrationLog() { }

        public OrchestrationLog(WorkItem wi)
        {
            Action = wi.Action;
            Current = wi.Current;
            Cache = wi.Cache;
            RecordId = wi.RecordId;
            ResourceId = wi.ResourceId;
            RecordType = wi.RecordType;
            Level = LogLevel.Warn;
        }

        public OrchestrationLog(OrderCloudException ex)
        {
            Level = LogLevel.Error;
            OrderCloudErrors = ex.Errors;
        }

        public OrchestrationLog(OrchestrationException ex) { }

        public OrchestrationLog(FunctionFailedException ex) { }

        public OrchestrationLog(Exception ex) { }

        [Sortable]
        public OrchestrationErrorType? ErrorType { get; set; }

        public string Message { get; set; } = string.Empty;

        [Sortable]
        public LogLevel Level { get; set; }

        [Sortable]
        public string ResourceId { get; set; } = string.Empty;

        [Sortable]
        public string RecordId { get; set; } = string.Empty;

        [Sortable]
        public RecordType? RecordType { get; set; }

        [Sortable]
        public Action? Action { get; set; }

        [DocIgnore]
        public JObject Current { get; set; } = new JObject();

        [DocIgnore]
        public JObject Cache { get; set; } = new JObject();

        [DocIgnore]
        public JObject Diff { get; set; } = new JObject();

        public ApiError[] OrderCloudErrors { get; set; }
    }
}