using System;
using System.Collections.Generic;
using System.Text;
using NUnit.Framework;
using ordercloud.integrations.library;

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
            var list = new List<string>() {"Product", "Spec"};
            Assert.IsTrue(list.JoinString("|") == "Product|Spec");
        }
    }
}
