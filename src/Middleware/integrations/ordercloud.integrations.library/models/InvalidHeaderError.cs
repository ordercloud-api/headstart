using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    public class InvalidHeaderError
    {
        public InvalidHeaderError(string name)
        {
            Header = name;
        }
        public string Header { get; set; }
    }
}
