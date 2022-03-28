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
	public class CatalogAssignmentSyncTests
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
			string catalogId1,
			string catalogId2,
			string buyerId,
			string userId
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
						CatalogAssignments = new List<string>{ catalogId1 }
					}
				},
				// define a Catalog assignment that is assigned but shouldn't be
				new HsLocationUserGroup {
					Id = catalogId2,
					xp = new HsLocationUserGroupXp
					{
						Type = "Catalog"
					}
				},
			};

			// Make sure catalog exists (otherwise it won't try to assign)
			existingCatalogs.Items = new List<HsLocationUserGroup>();
			existingCatalog1.ID = catalogId1;
			existingCatalogs.Items.Add(existingCatalog1);
			existingCatalog2.ID = catalogId2;
			existingCatalogs.Items.Add(existingCatalog2);

			groupAssignments.Meta.Page = 1;
			groupAssignments.Meta.TotalPages = 1;
			oc.UserGroups.ListUserAssignmentsAsync(buyerID: buyerId, userID: userId)
				.ReturnsForAnyArgs(groupAssignments.ToTask());
			var assignedGroupIDs = groupAssignments.Items.Select(g => g.UserGroupID);
			oc.UserGroups.ListAsync<HsLocationUserGroup>(buyerID: buyerId, filters: $"ID={string.Join("|", assignedGroupIDs)}", pageSize: 100)
				.Returns(assignedGroups.ToTask());
			oc.UserGroups.ListAsync<HsLocationUserGroup>(buyerId, filters: "xp.Type=Catalog", pageSize: 100)
				.Returns(existingCatalogs.ToTask());

			// Act
			if (!string.IsNullOrEmpty(buyerId) && !string.IsNullOrEmpty(userId))
			{
				await sut.SyncUserCatalogAssignments(buyerId, userId);
			}
			var userGroupAssignment = Arg.Is<UserGroupAssignment>(ass => 
				ass.UserGroupID == catalogId1 && ass.UserID == userId);
			if (!string.IsNullOrEmpty(buyerId) && userGroupAssignment != null)
			{
				await oc.UserGroups.Received().SaveUserAssignmentAsync(buyerId, userGroupAssignment);
			}
			if (!string.IsNullOrEmpty(buyerId) && !string.IsNullOrEmpty(catalogId2) && !string.IsNullOrEmpty(userId))
			{
				await oc.UserGroups.Received().DeleteUserAssignmentAsync(buyerId, catalogId2, userId);
			}
		}

		[Test, AutoNSubstituteData]
		public async Task SyncUserCatalogAssignments_ShouldNotTryToAssignIfCatalogDoesNotExist(
			[Frozen] IOrderCloudClient oc,
			HsCatalogCommand sut,
			ListPage<UserGroupAssignment> groupAssignments,
			ListPage<HsLocationUserGroup> assignedGroups,
			ListPage<HsLocationUserGroup> existingCatalogs,
			string catalogId1,
			string catalogId2,
			string buyerId,
			string userId
		)
		{
			// Arrange
			assignedGroups.Items = new List<HsLocationUserGroup>()
			{
				// Define an assigned group that is referencing a Catalog that should be assigned but isn't yet
				new HsLocationUserGroup
				{
					Id = "someid",
					xp = new HsLocationUserGroupXp
					{
						Type = "BuyerLocation",
						CatalogAssignments = new List<string>{ catalogId1 }
					}
				},
			};

			groupAssignments.Meta.Page = 1;
			groupAssignments.Meta.TotalPages = 1;
			oc.UserGroups.ListUserAssignmentsAsync(buyerID: buyerId, userID: userId)
				.ReturnsForAnyArgs(groupAssignments.ToTask());
			var assignedGroupIDs = groupAssignments.Items.Select(g => g.UserGroupID);
			oc.UserGroups.ListAsync<HsLocationUserGroup>(buyerID: buyerId, filters: $"ID={string.Join("|", assignedGroupIDs)}", pageSize: 100)
				.Returns(assignedGroups.ToTask());
			existingCatalogs.Items = new List<HsLocationUserGroup>();
			oc.UserGroups.ListAsync<HsLocationUserGroup>(buyerId, filters: "xp.Type=Catalog", pageSize: 100)
				.Returns(existingCatalogs.ToTask());

			// Act
			if (!string.IsNullOrEmpty(buyerId) && !string.IsNullOrEmpty(userId))
			{
				await sut.SyncUserCatalogAssignments(buyerId, userId);
			}
			var userGroupAssignment = Arg.Is<UserGroupAssignment>(ass => 
				ass.UserGroupID == catalogId1 && ass.UserID == userId);
			if (!string.IsNullOrEmpty(buyerId) && userGroupAssignment != null)
			{
				await oc.UserGroups.DidNotReceive().SaveUserAssignmentAsync(buyerId, userGroupAssignment);
			}
		}
	}
}