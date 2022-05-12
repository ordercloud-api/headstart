using Microsoft.Extensions.Logging;
using System.Collections.Generic;

namespace Headstart.Jobs
{
    /// <summary>
    /// This class adds logging and validation common in jobs.
    /// </summary>
    public abstract class BaseJob
    {
        protected List<string> Skipped { get; set; } = new List<string>();

        protected List<string> Succeeded { get; set; } = new List<string>();

        protected List<string> Failed { get; set; } = new List<string>();

        private int Total => Skipped.Count + Succeeded.Count + Failed.Count;

        protected ILogger _logger { get; set; }

        protected virtual void LogInformation(string message)
        {
            if (_logger != null)
            {
                _logger.LogInformation(message);
            }
        }

        protected virtual void LogSuccess(string message)
        {
            Succeeded.Add(message);
            if (_logger != null)
            {
                _logger.LogInformation($"Success -- {message}");
            }
        }

        protected virtual void LogFailure(string message)
        {
            Failed.Add(message);
            if (_logger != null)
            {
                _logger.LogError($"Failure -- {message}");
            }
        }

        protected virtual void LogSkip(string message)
        {
            Skipped.Add(message);
            if (_logger != null)
            {
                _logger.LogInformation($"Skipped -- {message}");
            }
        }

        protected virtual void LogProgress()
        {
            if (_logger != null)
            {
                _logger.LogInformation($"Found : {Total}. Failed: {Failed.Count}. Skipped: {Skipped.Count}. Succeeded: {Succeeded.Count}");
            }
        }
    }
}
