using System;

namespace Headstart.Common.Attributes
{
    public class OperatorSymbolAttribute : Attribute
    {
        public OperatorSymbolAttribute(string symbol)
        {
            this.Symbol = symbol;
        }

        public string Symbol { get; set; }
    }
}
