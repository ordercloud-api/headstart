using Headstart.Common.Exceptions;

namespace Headstart.Common.Models
{
    public class ProcessResultAction
    {
        public ProcessType ProcessType { get; set; }

        public string Description { get; set; }

        public bool Success { get; set; }

        public ProcessResultException Exception { get; set; }
    }
}
