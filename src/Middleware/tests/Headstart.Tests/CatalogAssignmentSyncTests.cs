using AutoFixture.NUnit3;
using NSubstitute;
using NUnit.Framework;
using OrderCloud.SDK;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Headstart.API.Commands;
using Headstart.Common.Models.Headstart;

namespace Headstart.Tests
{
	class CatalogAssignmentSyncTests
	{
		[Test, AutoNSubstituteData]
		public async Task SyncUserCatalogAssignments_ShouldHandleBasicScenario(
			[Frozen] IOrderCloudClient oc,
			HsCatalogCommand sut,
			ListPage<UserGroupAssignment> groupAssignments,
			ListPage<HsLocationUserGroup> assignedGroups,
			ListPage<HsLocationUserGroup> existingCatalogs,
			HsLocationUserGroup existingCatalog1,
			HsLocationUserGroup existingCatalog2,
			string catalogID1,
			string catalogID2,
			string buyerID,
			string userID
		)
		{
			// Arrange
			assignedGroups.Items = new List<HsLocationUserGroup>()
			{
				// define an assigned group that is referencing a Catalog that should be assigned but isn't yet
				new HsLocationUserGroup
				{
					Id = "someid",
					xp = new HsLocationUserGroupXp
					{
						Type = "BuyerLocation",
						CatalogAssignments = new List<string>{ catalogID1 }
					}
				},
				// define a Catalog assignment that is assigned but shouldn't be
				new HsLocationUserGroup {
					Id = catalogID2,
					xp = new HsLocationUserGroupXp
					{
						Type = "Catalog"
					}
				},
			};

			// make sure catalog exists (otherwise it won't try to assign)
			existingCatalogs.Items = new List<HsLocationUserGroup>();
			existingCatalog1.ID = catalogID1;
			existingCatalogs.Items.Add(existingCatalog1);
			existingCatalog2.ID = catalogID2;
			existingCatalogs.Items.Add(existingCatalog2);

			groupAssignments.Meta.Page = 1;
			groupAssignments.Meta.TotalPages = 1;
			oc.UserGroups.ListUserAssignmentsAsync(buyerID: buyerID, userID: userID)
				.ReturnsForAnyArgs(groupAssignments.ToTask());
			var assignedGroupIDs = groupAssignments.Items.Select(g => g.UserGroupID);
			oc.UserGroups.ListAsync<HsLocationUserGroup>(buyerID: buyerID, filters: $"ID={string.Join("|", assignedGroupIDs)}", pageSize: 100)
				.Returns(assignedGroups.ToTask());
			oc.UserGroups.ListAsync<HsLocationUserGroup>(buyerID, filters: "xp.Type=Catalog", pageSize: 100)
				.Returns(existingCatalogs.ToTask());

			// Act
			await sut.SyncUserCatalogAssignments(buyerID, userID);
			await oc.UserGroups.Received().SaveUserAssignmentAsync(buyerID, Arg.Is<UserGroupAssignment>(ass => ass.UserGroupID == catalogID1 && ass.UserID == userID));
			await oc.UserGroups.Received().DeleteUserAssignmentAsync(buyerID, catalogID2, userID);
		}

		[Test, AutoNSubstituteData]
		public async Task SyncUserCatalogAssignments_ShouldNotTryToAssignIfCatalogDoesNotExist(
			[Frozen] IOrderCloudClient oc,
			HsCatalogCommand sut,
			ListPage<UserGroupAssignment> groupAssignments,
			ListPage<HsLocationUserGroup> assignedGroups,
			ListPage<HsLocationUserGroup> existingCatalogs,
			string catalogID1,
			string catalogID2,
			string buyerID,
			string userID
		)
		{
			// Arrange
			assignedGroups.Items = new List<HsLocationUserGroup>()
			{
				// define an assigned group that is referencing a Catalog that should be assigned but isn't yet
				new HsLocationUserGroup
				{
					Id = "someid",
					xp = new HsLocationUserGroupXp
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
			oc.UserGroups.ListAsync<HsLocationUserGroup>(buyerID: buyerID, filters: $"ID={string.Join("|", assignedGroupIDs)}", pageSize: 100)
				.Returns(assignedGroups.ToTask());
			existingCatalogs.Items = new List<HsLocationUserGroup>();
			oc.UserGroups.ListAsync<HsLocationUserGroup>(buyerID, filters: "xp.Type=Catalog", pageSize: 100)
				.Returns(existingCatalogs.ToTask());

			// Act
			await sut.SyncUserCatalogAssignments(buyerID, userID);
			await oc.UserGroups.DidNotReceive().SaveUserAssignmentAsync(buyerID, Arg.Is<UserGroupAssignment>(ass => ass.UserGroupID == catalogID1 && ass.UserID == userID));
		}
	}
}
