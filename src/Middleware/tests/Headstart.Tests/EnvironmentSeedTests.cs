using Headstart.Common.Models.Misc;
using System.ComponentModel.DataAnnotations;
using NUnit.Framework;

namespace Headstart.Tests
{
    public class EnvironmentSeedTests
	{
		[Test]
        [TestCase("Aa@101234")] // less than 10 characters
        [TestCase("Aa@10123456789012345678901234567890123456789012345678901234567890123456789012345678901234567890123456")] // greater than 100 characters
        [TestCase("a@10123456")] // missing upper case
        [TestCase("A@10123456")] // missing lower case
        [TestCase("Aa@abcdefg")] // missing number
        [TestCase("Aa10123456")] // missing special character
        public void initial_admin_password_does_not_meet_minimum_requirements_fails_validation(string password)
		{
            // Arrange
            var seed = new EnvironmentSeed()
            {
                InitialAdminPassword = password
            };

            var ctx = new ValidationContext(seed)
            {
                MemberName = nameof(seed.InitialAdminPassword)
            };

            // Act
            var result = Validator.TryValidateProperty(seed.InitialAdminPassword, ctx, null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        [TestCase("Aa@1012345")]
        public void initial_admin_password_meets_minimum_requirements_passes_validation(string password)
        {
            // Arrange
            var seed = new EnvironmentSeed()
            {
                InitialAdminPassword = password
            };

            var ctx = new ValidationContext(seed)
            {
                MemberName = nameof(seed.InitialAdminPassword)
            };

            // Act
            var result = Validator.TryValidateProperty(seed.InitialAdminPassword, ctx, null);

            // Assert
            Assert.IsTrue(result);
        }

        [Test]
        [TestCase("other")]
        public void region_not_in_value_range_fails_validation(string region)
        {
            // Arrange
            var seed = new EnvironmentSeed()
            {
                Region = region
            };

            var ctx = new ValidationContext(seed)
            {
                MemberName = nameof(seed.Region)
            };

            // Act
            var result = Validator.TryValidateProperty(seed.Region, ctx, null);

            // Assert
            Assert.IsFalse(result);
        }

        [Test]
        [TestCase("")]
        [TestCase(null)]
        [TestCase("US-East")]
        [TestCase("Australia-East")]
        [TestCase("Europe-West")]
        [TestCase("Japan-East")]
        [TestCase("US-West")]
        public void region_in_value_range_passes_validation(string region)
        {
            // Arrange
            var seed = new EnvironmentSeed()
            {
                Region = region
            };

            var ctx = new ValidationContext(seed)
            {
                MemberName = nameof(seed.Region)
            };

            // Act
            var result = Validator.TryValidateProperty(seed.Region, ctx, null);

            // Assert
            Assert.IsTrue(result);
        }
    }
}
