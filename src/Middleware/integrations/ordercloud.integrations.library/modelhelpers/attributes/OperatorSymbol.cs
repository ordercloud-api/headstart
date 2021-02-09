using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    public class OperatorSymbol : Attribute
    {

        public OperatorSymbol(string symbol)
        {
            this.Symbol = symbol;
        }


        public string Symbol { get; set; }
    }
}
