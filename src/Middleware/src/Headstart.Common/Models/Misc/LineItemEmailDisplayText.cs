using System.Collections.Generic;
using Headstart.Models.Exceptions;
using ordercloud.integrations.library;

namespace Headstart.Models
{
    public class EmailDisplayText
    {
        public string EmailSubject { get; set; }
        public string DynamicText { get; set; }
        public string DynamicText2 { get; set; }
    }
}
