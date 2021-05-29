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
using Headstart.API.Commands.Crud;
using System.Linq;
using AutoFixture.NUnit3;

namespace Headstart.Tests
{
    class CatalogAssignmentSyncTests
    {

        [Test, AutoNSubstituteData]
        public async Task SyncUserCatalogAssignments_ShouldHandleBasicScenario(
            [Frozen] IOrderCloudClient oc,
            HSCatalogCommand sut,
            ListPage<UserGroupAssignment> groupAssignments,
            ListPage<HSLocationUserGroup> assignedGroups,
            ListPage<HSLocationUserGroup> existingCatalogs,
            HSLocationUserGroup existingCatalog1,
            HSLocationUserGroup existingCatalog2,
            string catalogID1,
            string catalogID2,
            string buyerID,
            string userID
        )
        {
            // Arrange
            assignedGroups.Items = new List<HSLocationUserGroup>()
            {
                // define an assigned group that is referencing a Catalog that should be assigned but isn't yet
                new HSLocationUserGroup
                {
                    ID = "someid",
                    xp = new HSLocationUserGroupXp
                    {
                        Type = "BuyerLocation",
                        CatalogAssignments = new List<string>{ catalogID1 }
                    }
                },
                // define a Catalog assignment that is assigned but shouldn't be
                new HSLocationUserGroup {
                    ID = catalogID2,
                    xp = new HSLocationUserGroupXp
                    {
                        Type = "Catalog"
                    }
                },
            };

            // make sure catalog exists (otherwise it won't try to assign)
            existingCatalogs.Items = new List<HSLocationUserGroup>();
            existingCatalog1.ID = catalogID1;
            existingCatalogs.Items.Add(existingCatalog1);
            existingCatalog2.ID = catalogID2;
            existingCatalogs.Items.Add(existingCatalog2);

            groupAssignments.Meta.Page = 1;
            groupAssignments.Meta.TotalPages = 1;
            oc.UserGroups.ListUserAssignmentsAsync(buyerID: buyerID, userID: userID)
                .ReturnsForAnyArgs(groupAssignments.ToTask());
            var assignedGroupIDs = groupAssignments.Items.Select(g => g.UserGroupID);
            oc.UserGroups.ListAsync<HSLocationUserGroup>(buyerID: buyerID, filters: $"ID={string.Join("|", assignedGroupIDs)}", pageSize: 100)
                .Returns(assignedGroups.ToTask());
            oc.UserGroups.ListAsync<HSLocationUserGroup>(buyerID, filters: "xp.Type=Catalog", pageSize: 100)
                .Returns(existingCatalogs.ToTask());

            // Act
            await sut.SyncUserCatalogAssignments(buyerID, userID);
            await oc.UserGroups.Received().SaveUserAssignmentAsync(buyerID, Arg.Is<UserGroupAssignment>(ass => ass.UserGroupID == catalogID1 && ass.UserID == userID));
            await oc.UserGroups.Received().DeleteUserAssignmentAsync(buyerID, catalogID2, userID);
        }

        [Test, AutoNSubstituteData]
        public async Task SyncUserCatalogAssignments_ShouldNotTryToAssignIfCatalogDoesNotExist(
            [Frozen] IOrderCloudClient oc,
            HSCatalogCommand sut,
            ListPage<UserGroupAssignment> groupAssignments,
            ListPage<HSLocationUserGroup> assignedGroups,
            ListPage<HSLocationUserGroup> existingCatalogs,
            string catalogID1,
            string catalogID2,
            string buyerID,
            string userID
        )
        {
            // Arrange
            assignedGroups.Items = new List<HSLocationUserGroup>()
            {
                // define an assigned group that is referencing a Catalog that should be assigned but isn't yet
                new HSLocationUserGroup
                {
                    ID = "someid",
                    xp = new HSLocationUserGroupXp
                    {
                        Type = "BuyerLocation",
                        CatalogAssignments = new List<string>{ catalogID1 }
                    }
                },
            };

            groupAssignments.Meta.Page = 1;
            groupAssignments.Meta.TotalPages = 1;
            oc.UserGroups.ListUserAssignmentsAsync(buyerID: buyerID, userID: userID)
                .ReturnsForAnyArgs(groupAssignments.ToTask());
            var assignedGroupIDs = groupAssignments.Items.Select(g => g.UserGroupID);
            oc.UserGroups.ListAsync<HSLocationUserGroup>(buyerID: buyerID, filters: $"ID={string.Join("|", assignedGroupIDs)}", pageSize: 100)
                .Returns(assignedGroups.ToTask());
            existingCatalogs.Items = new List<HSLocationUserGroup>();
            oc.UserGroups.ListAsync<HSLocationUserGroup>(buyerID, filters: "xp.Type=Catalog", pageSize: 100)
                .Returns(existingCatalogs.ToTask());

            // Act
            await sut.SyncUserCatalogAssignments(buyerID, userID);
            await oc.UserGroups.DidNotReceive().SaveUserAssignmentAsync(buyerID, Arg.Is<UserGroupAssignment>(ass => ass.UserGroupID == catalogID1 && ass.UserID == userID));
        }
    }
}
