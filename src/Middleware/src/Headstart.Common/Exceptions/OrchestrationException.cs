using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Headstart.Common.Models;
using Newtonsoft.Json.Converters;

namespace Headstart.Common.Exceptions
{
	public class OrchestrationException : Exception
	{
		private OrchestrationError Error { get; set; } = new OrchestrationError();

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
		[JsonConverter(typeof(StringEnumConverter))]
		public OrchestrationErrorType Type { get; set; } = new OrchestrationErrorType();

		public string RecordId { get; set; } = string.Empty;

		public string SupplierId { get; set; } = string.Empty;

		[JsonConverter(typeof(StringEnumConverter))]
		public RecordType RecordType { get; set; }

		[JsonConverter(typeof(StringEnumConverter))]
		public Models.Action Action { get; set; }

		public JObject Current { get; set; } = new JObject();

		public JObject Cache { get; set; } = new JObject();

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