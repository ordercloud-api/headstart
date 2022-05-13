using System;

namespace OrderCloud.Integrations.Library.Attributes
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
