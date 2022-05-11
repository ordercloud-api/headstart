using Headstart.Common.Mappers;
using Headstart.Common.Models;
using NUnit.Framework;
using ordercloud.integrations.exchangerates;
using System;
using System.Collections.Generic;
using System.Text;

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
        public void encoding_and_decoding_state(string testString)
        {
            var state = new SSOState() { Path = testString };
            var afterCoding = Coding.DecodeState(Coding.EncodeState(state));
            Assert.AreEqual(testString, afterCoding.Path);
        }

        [Test]
        [TestCase("US ", CurrencySymbol.USD)]
        [TestCase(" usa  ", CurrencySymbol.USD)]
        [TestCase("  CAN ", CurrencySymbol.CAD)]
        [TestCase(" ca", CurrencySymbol.CAD)]
        public void get_curreny_from_country(string country, CurrencySymbol expectedCurrency)
        {
            var currency = Geography.GetCurrency(country);
            Assert.AreEqual(expectedCurrency, currency);
        }

        [Test]
        [TestCase(null)]
        [TestCase("sadfasdfsdfasd")]
        public void get_curreny_from_should_fail(string country)
        {
            Assert.Throws<Exception>(() => Geography.GetCurrency(country));
        }

        [Test]
        [TestCase("US ", "US")]
        [TestCase(" usa  ", "US")]
        [TestCase("  CAN ", "CA")]
        [TestCase(" ca", "CA")]
        public void get_country_code(string country, string expectedCode)
        {
            var countryCode = Geography.GetCountry(country);
            Assert.AreEqual(expectedCode, countryCode);
        }

        [Test]
        [TestCase(null)]
        [TestCase("sdfsfsdf")]
        public void get_country_should_fail(string country)
        {
            Assert.Throws<Exception>(() => Geography.GetCountry(country));
        }

        [Test]
        [TestCase("Georgia", "GA")]
        [TestCase("WASHINGTON", "WA")]
        [TestCase("iowa", "IA")]
        [TestCase("  QUEBEC  ", "QC")]
        public void get_state_abreviation(string stateName, string expectedAbreviation)
        {
            var abreviation = Geography.GetStateAbreviationFromName(stateName);
            Assert.AreEqual(expectedAbreviation, abreviation);
        }

        [Test]
        [TestCase(null)]
        [TestCase("sdfsdfsdfsd")]
        public void get_state_abreviation_should_fail(string state)
        {
            Assert.Throws<Exception>(() => Geography.GetStateAbreviationFromName(state));
        }
    }
}
