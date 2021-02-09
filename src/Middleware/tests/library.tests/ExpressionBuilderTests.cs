using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using ordercloud.integrations.library;

namespace library.tests
{
    public class ExpressionBuilderTests
    {
        [SetUp]
        public void Setup() { }


        [Test, TestCaseSource(typeof(ParseFactory), nameof(ParseFactory.TestCases))]
        public void list_filter_parse_test(string expression, dynamic filter)
        {
            var parse = ListFilter.Parse("", expression).Values.First();
            Assert.IsTrue(parse.Operator == filter.Operator);
            Assert.IsTrue(parse.Term == filter.Term);
            Assert.IsTrue(parse.HasWildcard == filter.HasWildcard);
            for (var i = 0; i <= filter.WildcardPositions.Count - 1; i++)
            {
                Assert.AreEqual(parse.WildcardPositions[i], filter.WildcardPositions[i]);
            }
        }
    }

    public class ParseFactory
    {
        public static IEnumerable<TestCaseData> TestCases
        {
            get
            {
                yield return new TestCaseData("!todd*", new
                {
                    Operator = ListFilterOperator.NotEqual,
                    Term = "todd",
                    HasWildcard = true,
                    WildcardPositions = new List<int>() { 4 }
                });
                yield return new TestCaseData("*todd", new
                {
                    Operator = ListFilterOperator.Equal,
                    Term = "todd",
                    HasWildcard = true,
                    WildcardPositions = new List<int>() { 0 }
                });
                yield return new TestCaseData("todd*", new
                {
                    Operator = ListFilterOperator.Equal,
                    Term = "todd",
                    HasWildcard = true,
                    WildcardPositions = new List<int>() { 4 }
                });
                yield return new TestCaseData("*todd*", new
                {
                    Operator = ListFilterOperator.Equal,
                    Term = "todd",
                    HasWildcard = true,
                    WildcardPositions = new List<int>() { 0, 4 }
                });
                yield return new TestCaseData(">=todd", new
                {
                    Operator = ListFilterOperator.GreaterThanOrEqual,
                    Term = "todd",
                    HasWildcard = false,
                    WildcardPositions = new List<int>() { }
                });
                yield return new TestCaseData("<=todd", new
                {
                    Operator = ListFilterOperator.LessThanOrEqual,
                    Term = "todd",
                    HasWildcard = false,
                    WildcardPositions = new List<int>() { }
                });
                yield return new TestCaseData("!todd", new
                {
                    Operator = ListFilterOperator.NotEqual,
                    Term = "todd",
                    HasWildcard = false,
                    WildcardPositions = new List<int>() { }
                });
                yield return new TestCaseData("=todd", new
                {
                    Operator = ListFilterOperator.Equal,
                    Term = "todd",
                    HasWildcard = false,
                    WildcardPositions = new List<int>() { }
                });
                yield return new TestCaseData(">todd", new
                {
                    Operator = ListFilterOperator.GreaterThan,
                    Term = "todd",
                    HasWildcard = false,
                    WildcardPositions = new List<int>() { }
                });
                yield return new TestCaseData("<todd", new
                {
                    Operator = ListFilterOperator.LessThan,
                    Term = "todd",
                    HasWildcard = false,
                    WildcardPositions = new List<int>() { }
                });
            }
        }
    }
}