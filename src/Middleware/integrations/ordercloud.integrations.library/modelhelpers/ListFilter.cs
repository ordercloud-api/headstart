using System;
using System.Collections.Generic;
using System.Text;

namespace ordercloud.integrations.library
{
    public class ListFilter
    {
        public string Name { get; set; }

        /// <summary>
        /// If multiple, OR them together
        /// </summary>
        public IList<ListFilterValue> Values { get; set; } = new List<ListFilterValue>();
        public IList<Tuple<string, string>> QueryParams = new List<Tuple<string, string>>();

        // used in ModelBinder
        public static ListFilter Parse(string name, string expression)
        {
            var result = new ListFilter { Name = name };
            var value = new ListFilterValue();
            var escape = false;
            var negate = false;

            foreach (var c in expression)
            {
                if (escape)
                {
                    value.Term += c.ToString();
                    escape = false;
                }
                else if (c == '\\')
                {
                    escape = true;
                }
                else if (c == '!' && value.Term == "")
                {
                    value.Operator = ListFilterOperator.NotEqual;
                    negate = true;
                }
                else if (c == '>' && value.Term == "")
                {
                    value.Operator = negate ? ListFilterOperator.LessThanOrEqual : ListFilterOperator.GreaterThan;
                }
                else if (c == '<' && value.Term == "")
                {
                    value.Operator = negate ? ListFilterOperator.GreaterThanOrEqual : ListFilterOperator.LessThan;
                }
                else if (c == '=' && value.Operator == ListFilterOperator.GreaterThan && value.Term == "")
                {
                    value.Operator = ListFilterOperator.GreaterThanOrEqual;
                }
                else if (c == '=' && value.Operator == ListFilterOperator.LessThan && value.Term == "")
                {
                    value.Operator = ListFilterOperator.LessThanOrEqual;
                }
                else if (c == '=' && value.Term == "")
                {
                    value.Operator = ListFilterOperator.Equal;
                }
                else if (c == '*')
                {
                    value.WildcardPositions.Add(value.Term.Length);
                }
                else if (c == '|')
                {
                    result.Values.Add(value);
                    value = new ListFilterValue();
                    negate = false;
                }
                else
                {
                    value.Term += c.ToString();
                }
            }
            result.QueryParams.Add(new Tuple<string, string>(name, expression));
            result.Values.Add(value);
            return result;
        }
    }
}
