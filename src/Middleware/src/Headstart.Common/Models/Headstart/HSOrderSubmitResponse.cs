using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Collections.Generic;
using Headstart.Common.Exceptions;

namespace Headstart.Common.Models.Headstart
{
	public class OrderSubmitResponseXp
	{
		public List<ProcessResult> ProcessResults { get; set; } = new List<ProcessResult>();
	}

	public class ProcessResult
	{
		public ProcessType Type { get; set; }

		public List<ProcessResultAction> Activity { get; set; } = new List<ProcessResultAction>();
	}

	public class ProcessResultAction
	{
		public ProcessType ProcessType { get; set; }

		public string Description { get; set; } = string.Empty;

		public bool Success { get; set; }

		public ProcessResultException Exception { get; set; }
	}

	[JsonConverter(typeof(StringEnumConverter))]
	public enum ProcessType
	{
		Patching,
		Forwarding,
		Notification,
		Accounting,
		Tax,
		Shipping
	}
}