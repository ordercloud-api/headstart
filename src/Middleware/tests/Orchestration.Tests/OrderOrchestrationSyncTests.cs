using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common;
using Headstart.Common.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using NSubstitute;
using NUnit.Framework;
using OrderCloud.SDK;

namespace Orchestration.Tests
{
    public class OrderOrchestrationSyncTests
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

        [Test]
        [Ignore("Setup Base Url cannot be null")]
        public async Task ensure_order_sync_get_returns_jobject()
        {
            mockOrderCloudClient.Me.GetAsync().ReturnsForAnyArgs(new MeUser()
            {
                Active = true,
                Username = "oliver",
                ID = "oliver",
                Email = "oliver@email.com",
                Supplier = new MeSupplier() { ID = "xxx_bad_xxx" },
                AvailableRoles = new List<string>()
                {
                    "FullAccess"
                }
            });
            
            var functionToken = new OrderCloudIntegrationsFunctionToken(Substitute.For<AppSettings>());
            var verifiedUser = await functionToken.Authorize(mockHttpRequest, new[] { ApiRole.OrderAdmin }, mockOrderCloudClient);
            var command = new SupplierSyncCommand(Substitute.For<AppSettings>());
            Assert.ThrowsAsync<MissingMethodException>(async () => await command.GetOrderAsync("ID", verifiedUser));
        }
    }
}
