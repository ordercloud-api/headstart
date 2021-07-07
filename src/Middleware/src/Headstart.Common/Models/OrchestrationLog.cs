using System;
using Cosmonaut.Attributes;
using Headstart.Common.Exceptions;
using Microsoft.Azure.WebJobs;
using Newtonsoft.Json.Linq;
using ordercloud.integrations.library;
using ordercloud.integrations.library.helpers;
using OrderCloud.SDK;

namespace Headstart.Common.Models
{
    
	[CosmosCollection("orchestrationlogs")]
	public class OrchestrationLog : CosmosObject
    {
        public OrchestrationLog()
        {
        }

        public OrchestrationLog(WorkItem wi)
        {
            this.Action = wi.Action;
            this.Current = wi.Current;
            this.Cache = wi.Cache;
            this.RecordId = wi.RecordId;
            this.ResourceId = wi.ResourceId;
            this.RecordType = wi.RecordType;
            this.Level = LogLevel.Warn;
        }

        public OrchestrationLog(OrderCloudException ex)
        {
            this.Level = LogLevel.Error;
            this.OrderCloudErrors = ex.Errors;
        }

        public OrchestrationLog(OrchestrationException ex)
        {

        }

        public OrchestrationLog(FunctionFailedException ex)
        {

        }

        public OrchestrationLog(Exception ex)
        {

        }

        [Sortable]
        public OrchestrationErrorType? ErrorType { get; set; }
        public string Message { get; set; }
        [Sortable]
        public LogLevel Level { get; set; }
        [Sortable]
        public string ResourceId { get; set; }
        [Sortable]
        public string RecordId { get; set; }
        [Sortable]
        public RecordType? RecordType { get; set; }
        [Sortable]
        public Action? Action { get; set; }
        [DocIgnore]
        public JObject Current { get; set; }
        [DocIgnore]
        public JObject Cache { get; set; }
        [DocIgnore]
        public JObject Diff { get; set; }
        public OrderCloud.SDK.ApiError[] OrderCloudErrors { get; set; }
    }
}
