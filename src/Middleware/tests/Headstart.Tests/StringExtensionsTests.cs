using System;
using System.Collections.Generic;
using Headstart.Common.Extensions;
using NUnit.Framework;

namespace Headstart.Tests
{
    public class ExtensionsTests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void JoinString_WithStringList_ConcanatesListWithSeparator()
        {
            // Arrange
            var list = new List<string>() { "Product", "Spec" };

            // Act
            var joinedString = list.JoinString("|");

            // Assert
            Assert.IsTrue(joinedString == "Product|Spec");
        }

        [Test]
        public void JoinString_WithEnumList_ConcanatesListWithSeparator()
        {
            // Arrange
            var list = new List<ConsoleColor>() { ConsoleColor.Black, ConsoleColor.Blue };

            // Act
            var joinedString = list.JoinString("|");

            // Assert
            Assert.IsTrue(joinedString == "Black|Blue");
        }
    }
}
