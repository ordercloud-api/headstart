using System;
using Headstart.Common.Mappers;
using Headstart.Common.Models;
using NUnit.Framework;

namespace Headstart.Tests
{
    public class GeographyTests
    {
        [Test]
        [TestCase("test_example")]
        [TestCase("/dsf/df//df/e/")]
        [TestCase("12345!@#$%^&*()")]
        [TestCase("")]
        [TestCase(null)]
        public void DecodeStateEncodeState_WithSameSSOState_ReturnsTheOriginalValue(string testString)
        {
            // Arrange
            var state = new SSOState() { Path = testString };

            // Act
            var afterCoding = Coding.DecodeState(Coding.EncodeState(state));

            // Assert
            Assert.AreEqual(testString, afterCoding.Path);
        }

        [Test]
        [TestCase("US ", CurrencyCode.USD)]
        [TestCase(" usa  ", CurrencyCode.USD)]
        [TestCase("  CAN ", CurrencyCode.CAD)]
        [TestCase(" ca", CurrencyCode.CAD)]
        public void GetCurrency_ForSupportedCountryCode_ReturnsCorrectCurrency(string country, CurrencyCode expectedCurrency)
        {
            // Arrange

            // Act
            var currency = Geography.GetCurrency(country);

            // Assert
            Assert.AreEqual(expectedCurrency, currency);
        }

        [Test]
        [TestCase(null)]
        [TestCase("sadfasdfsdfasd")]
        public void GetCurrency_ForUnsupportedCountryCode_ThrowsException(string country)
        {
            // Arrange

            // Act/Assert
            Assert.Throws<Exception>(() => Geography.GetCurrency(country));
        }

        [Test]
        [TestCase("US ", "US")]
        [TestCase(" usa  ", "US")]
        [TestCase("  CAN ", "CA")]
        [TestCase(" ca", "CA")]
        public void GetCountry_ForSupportedCountryCode_ReturnsParsedCountryCode(string country, string expectedCode)
        {
            // Arrange

            // Act
            var countryCode = Geography.GetCountry(country);

            // Assert
            Assert.AreEqual(expectedCode, countryCode);
        }

        [Test]
        [TestCase(null)]
        [TestCase("sdfsfsdf")]
        public void GetCountry_ForUnsupportedCountryCode_ThrowsException(string country)
        {
            // Arrange

            // Act/Assert
            Assert.Throws<Exception>(() => Geography.GetCountry(country));
        }

        [Test]
        [TestCase("Georgia", "GA")]
        [TestCase("WASHINGTON", "WA")]
        [TestCase("iowa", "IA")]
        [TestCase("  QUEBEC  ", "QC")]
        public void GetStateAbreviationFromName_WithSupportedStateName_ReturnsStateCode(string stateName, string expectedAbreviation)
        {
            // Arrange

            // Act
            var abreviation = Geography.GetStateAbreviationFromName(stateName);

            // Assert
            Assert.AreEqual(expectedAbreviation, abreviation);
        }

        [Test]
        [TestCase(null)]
        [TestCase("sdfsdfsdfsd")]
        public void GetStateAbreviationFromName_WithUnsupportedStateName_ThrowsException(string state)
        {
            // Arrange

            // Act/Assert
            Assert.Throws<Exception>(() => Geography.GetStateAbreviationFromName(state));
        }
    }
}
