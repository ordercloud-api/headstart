using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    public enum ListFilterOperator
    {
        [OperatorSymbol("=")]
        Equal,
        [OperatorSymbol(">")]
        GreaterThan,
        [OperatorSymbol("<")]
        LessThan,
        [OperatorSymbol(">=")]
        GreaterThanOrEqual,
        [OperatorSymbol("<=")]
        LessThanOrEqual,
        [OperatorSymbol("<>")]
        NotEqual
    }
}
