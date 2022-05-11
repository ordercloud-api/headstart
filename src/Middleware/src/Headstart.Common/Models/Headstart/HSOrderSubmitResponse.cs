using System;
using System.Collections.Generic;
using System.Text;
using Headstart.Common.Exceptions;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using OrderCloud.SDK;

namespace Headstart.Models.Headstart
{

    public class OrderSubmitResponseXp
    {
        public List<ProcessResult> ProcessResults { get; set; }
    }

    public class ProcessResult
    {
        public ProcessType Type { get; set; }
        public List<ProcessResultAction> Activity { get; set; } = new List<ProcessResultAction>();
    }

    public class ProcessResultAction
    {
        public ProcessType ProcessType { get; set; }
        public string Description { get; set; }
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
