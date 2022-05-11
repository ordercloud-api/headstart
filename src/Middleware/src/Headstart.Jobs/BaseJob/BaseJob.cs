using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Headstart.Jobs
{
    /// <summary>
    /// This class adds logging and validation common in jobs
    /// </summary>
    public abstract class BaseJob
    {
        protected List<string> Skipped = new List<string>();
        protected List<string> Succeeded = new List<string>();
        protected List<string> Failed = new List<string>();
        private int Total => Skipped.Count + Succeeded.Count + Failed.Count;
        protected ILogger _logger;

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
            if(_logger != null)
            {
                _logger.LogInformation($"Success -- {message}");
            }
        }

        protected virtual void LogFailure(string message)
        {
            Failed.Add(message);
            if(_logger != null)
            {
                _logger.LogError($"Failure -- {message}");
            }
        }

        protected virtual void LogSkip(string message)
        {
            Skipped.Add(message);
            if(_logger != null)
            {
                _logger.LogInformation($"Skipped -- {message}");
            }
        }

        protected virtual void LogProgress()
        {
            if(_logger != null)
            {
                _logger.LogInformation($"Found : {Total}. Failed: {Failed.Count}. Skipped: {Skipped.Count}. Succeeded: {Succeeded.Count}");
            }
        }
    }
}
