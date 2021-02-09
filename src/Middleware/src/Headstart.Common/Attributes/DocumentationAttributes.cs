using System;
using ordercloud.integrations.library;

namespace Headstart.Models.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public sealed class HSSection : DocSection
    {
        public HSSection()
        {
            
        }
        // <summary>
        /// Use on controllers to indicate that they belong in the Orchestration section of the API reference docs.
        /// </summary>
        public class OrchestrationAttribute : DocSection { }
        /// <summary>
        /// Use on controllers to indicate that they belong in the Headstart section of the API reference docs.
        /// </summary>
        public class HeadstartAttribute : DocSection { }

        public class IntegrationAttribute : DocSection { }

		/// <summary>
		/// Use on controllers to indicate that they belong in the Content section of the API reference docs.
		/// </summary>
		public class ContentAttribute : DocSection { }
	}
}
