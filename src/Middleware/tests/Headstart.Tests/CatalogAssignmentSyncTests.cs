using NUnit.Framework;
using NSubstitute;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using AutoFixture;
using OrderCloud.SDK;
using Headstart.Models;
using Headstart.Common;
using ordercloud.integrations.library.helpers;
using Headstart.API.Commands.Crud;

namespace Headstart.Tests
{
    class CatalogAssignmentSyncTests
    {
        private IOrderCloudClient _oc;
        private AppSettings _settings;
        private IHSCatalogCommand _sut;


        [SetUp]
        public void Setup()
        {
            _oc = Substitute.For<IOrderCloudClient>();
            _settings = Substitute.For<AppSettings>();
            _sut = new HSCatalogCommand(_settings, _oc);
        }

        [Test]
        public async Task TestCatalogAssignmentSync()
        {
            //arrange
            var buyerId = "buyer1";
            var useriD = "user1";
            var currentLocationCatalogLists = new List<List<string>>()
            {
                new List<string>()
                {
                    "123", "456"
                },
                new List<string>()
                {
                    "456, 789", "910"
                }
            };
            var currentCatalogAssignments = new List<string>()
            {
                "123", "456", "789", "000"
            };

            var currentLocationPermissionAssignments = new List<string>()
            {
                "321"
            };
            _oc.UserGroups.ListUserAssignmentsAsync(buyerID: Arg.Any<string>(), userID: Arg.Any<string>(), page: Arg.Any<int>(), pageSize: Arg.Any<int>())
                .ReturnsForAnyArgs(Task.FromResult(GetUserGroupAssignments()));
            _oc.UserGroups.ListAsync<HSLocationUserGroup>(buyerID: Arg.Any<string>(), filters: Arg.Any<string>())
                .ReturnsForAnyArgs(GetUserGroups(currentLocationCatalogLists, currentCatalogAssignments, currentLocationPermissionAssignments));


            //act
            await _sut.SyncUserCatalogAssignments(buyerId, useriD);

            //assert
            await _oc.UserGroups.Received().SaveUserAssignmentAsync(buyerId, Arg.Is<UserGroupAssignment>(assignment => assignment.UserGroupID == "910"));
            await _oc.UserGroups.Received().DeleteUserAssignmentAsync(buyerId, "000", useriD);
            await _oc.UserGroups.DidNotReceive().DeleteUserAssignmentAsync(buyerId, "321", useriD);

        }

        private ListPage<UserGroupAssignment> GetUserGroupAssignments()
        {
            Fixture fixture = new Fixture();
            return fixture.Create<ListPage<UserGroupAssignment>>();
        }

        private ListPage<HSLocationUserGroup> GetUserGroups(List<List<string>> catalogAssignmentList, List<string> catalogsAssigned, List<string> locationPermissionsAssigned)
        {
            var userGroupList = new List<HSLocationUserGroup>();
            catalogAssignmentList.ForEach(catalogList =>
            {
                userGroupList.Add(GetBuyerLocationUserGroup(catalogList));
            });
            catalogsAssigned.ForEach(catalogID =>
            {
                userGroupList.Add(GetGenericUserGroup(catalogID, "Catalog"));
            });
            locationPermissionsAssigned.ForEach(catalogID =>
            {
                userGroupList.Add(GetGenericUserGroup(catalogID, "LocationPermissions"));
            });
            return new ListPage<HSLocationUserGroup>()
            {
                Items = userGroupList
            };
        }

        private HSLocationUserGroup GetBuyerLocationUserGroup(List<string> catalogAssignments)
        {
            var userGroup = new HSLocationUserGroup()
            {
                xp = new HSLocationUserGroupXp()
                {
                    Type = "BuyerLocation",
                    CatalogAssignments = catalogAssignments
                }
            };
            return userGroup;
        }

        private HSLocationUserGroup GetGenericUserGroup(string id, string catalogType)
        {
            var userGroup = new HSLocationUserGroup()
            {
                ID = id,
                xp = new HSLocationUserGroupXp()
                {
                    Type = catalogType
                }
            };
            return userGroup;
        }
    }
}
