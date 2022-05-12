using System;

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
