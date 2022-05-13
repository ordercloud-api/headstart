using System;

namespace OrderCloud.Integrations.Library.Attributes
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
