using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.Common;
using Headstart.Common.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using NUnit.Framework;
using OrderCloud.SDK;

namespace Headstart.Tests
{
    public class FunctionTokenTests
    {
        private const string token = "eyJ0eXAiOiJKV1QiLCJhbGciOiJIUzI1NiJ9.eyJ1c3IiOiJvbGl2ZXIiLCJjaWQiOiIwNmM5MzYyOS1mZTlhLTRlYzUtOTY1Mi1jMGYwNTliNWNjN2MiLCJ1IjoiMjg0NDMxNSIsInVzcnR5cGUiOiJzdXBwbGllciIsInJvbGUiOlsiUHJvZHVjdEFkbWluIiwiUHJpY2VTY2hlZHVsZUFkbWluIiwiU3VwcGxpZXJSZWFkZXIiLCJPcmRlckFkbWluIiwiU3VwcGxpZXJVc2VyQWRtaW4iLCJTdXBwbGllclVzZXJHcm91cEFkbWluIiwiU3VwcGxpZXJBZGRyZXNzQWRtaW4iLCJQcm9kdWN0RmFjZXRSZWFkZXIiLCJTaGlwbWVudEFkbWluIiwiTVBNZVByb2R1Y3RBZG1pbiIsIk1QT3JkZXJBZG1pbiIsIk1QU2hpcG1lbnRBZG1pbiIsIk1QTWVTdXBwbGllckFkZHJlc3NBZG1pbiIsIk1QTWVTdXBwbGllclVzZXJBZG1pbiIsIk1QU3VwcGxpZXJVc2VyR3JvdXBBZG1pbiJdLCJpc3MiOiJodHRwczovL3N0YWdpbmdhdXRoLm9yZGVyY2xvdWQuaW8iLCJhdWQiOiJodHRwczovL3N0YWdpbmdhcGkub3JkZXJjbG91ZC5pbyIsImV4cCI6MTU5MDAyNTM2OSwibmJmIjoxNTg5OTg5MzY5fQ.qh7QrDAAe4pfLXVOovHlfcwE25-i23pTvAbSSWtZaAg";
        private HttpRequest mockHttpRequest;
        private IOrderCloudClient mockOrderCloudClient;

        [SetUp]
        public void Setup()
        {
            mockHttpRequest = Substitute.For<HttpRequest>();
            mockHttpRequest.Headers.Returns(new HeaderDictionary()
            {
                new KeyValuePair<string, StringValues>("Authorization", $"Bearer {token}")
            });
            mockOrderCloudClient = Substitute.For<IOrderCloudClient>();
        }

        [Test, TestCaseSource(typeof(MeUserFactory), nameof(MeUserFactory.Users))]
        public async Task valid_token_returns_user(MeUser user)
        {
            mockOrderCloudClient.Me.GetAsync().ReturnsForAnyArgs(user);
            var functionToken = new OrderCloudIntegrationsFunctionToken(Substitute.For<AppSettings>());
            var verifiedUser = await functionToken.Authorize(mockHttpRequest, new[] { ApiRole.OrderAdmin }, mockOrderCloudClient);
            Assert.IsTrue(user.ID == verifiedUser.UserID);
            if (user.Supplier != null)
                Assert.IsTrue(user.Supplier.ID == verifiedUser.SupplierID);
            if (user.Buyer != null)
                Assert.IsTrue(user.Buyer.ID == verifiedUser.BuyerID);
            Assert.IsTrue(user.Email == verifiedUser.Email);
            Assert.IsTrue(user.Username == verifiedUser.Username);
        }

        [Test]
        public void inactive_user_throws_exception()
        {
            mockOrderCloudClient.Me.GetAsync().ReturnsForAnyArgs(new MeUser() { Active = false });

            var functionToken = new OrderCloudIntegrationsFunctionToken(Substitute.For<AppSettings>());
            Assert.ThrowsAsync<Exception>(async () => await functionToken.Authorize(mockHttpRequest, new[] { ApiRole.OrderAdmin }, mockOrderCloudClient));
        }
    }
    public class MeUserFactory
    {
        public static IEnumerable<MeUser> Users
        {
            get
            {
                yield return new MeUser()
                {
                    Active = true,
                    Username = "oliver",
                    ID = "oliver",
                    Email = "oliver@email.com",
                    Buyer = new MeBuyer() { ID = "buyerid" },
                    AvailableRoles = new List<string>()
                    {
                        "ProductAdmin"
                    }
                };
                yield return new MeUser()
                {
                    Active = true,
                    Username = "oliver",
                    ID = "oliver",
                    Email = "oliver@email.com",
                    Supplier = new MeSupplier() { ID = "buyerid" },
                    AvailableRoles = new List<string>()
                    {
                        "FullAccess"
                    }
                };
            }
        }
    }
}
