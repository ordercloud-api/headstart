using System;
using System.Collections.Generic;
using NUnit.Framework;
using ordercloud.integrations.library;

namespace library.tests
{
    public class ExtensionTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void join_string_words()
        {
            var list = new List<ConsoleColor>() { ConsoleColor.Black, ConsoleColor.Blue };
            Assert.IsTrue(list.JoinString("|") == "Black|Blue");
        }
    }
}
