﻿using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace Headstart.Jobs
{
    public abstract class BaseTimerJob : BaseJob
    {
        protected abstract bool ShouldRun { get; }

        public async Task Run(ILogger logger)
        {
            _logger = logger;
            if (!ShouldRun)
            {
                _logger.LogWarning("Skipping Job - ShouldRun evaluated to false");
                return;
            }

            try
            {
                await ProcessJob();
            }
            catch (Exception ex)
            {
                LogFailure(ex.Message);
            }

            LogProgress();

            if (Failed.Count > 0)
            {
                throw new Exception("There were one or more errors during job");
            }
        }

        protected abstract Task ProcessJob();
    }
}
