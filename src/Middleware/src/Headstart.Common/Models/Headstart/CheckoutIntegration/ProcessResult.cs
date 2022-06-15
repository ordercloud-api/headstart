using System.Collections.Generic;

namespace Headstart.Common.Models
{
    public class ProcessResult
    {
        public ProcessType Type { get; set; }

        public List<ProcessResultAction> Activity { get; set; } = new List<ProcessResultAction>();
    }
}
