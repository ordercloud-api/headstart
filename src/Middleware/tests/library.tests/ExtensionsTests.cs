using System.Collections.Generic;
using NUnit.Framework;
using OrderCloud.Integrations.Library;

namespace library.tests
{
    public class ExtensionsTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void join_string_words()
        {
            var list = new List<string>() { "Product", "Spec" };
            Assert.IsTrue(list.JoinString("|") == "Product|Spec");
        }
    }
}
