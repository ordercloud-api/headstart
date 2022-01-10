import { t } from 'testcafe'
import testConfig from '../../testConfig'
import { adminTestSetup, adminClientSetup, buyerTestSetup } from '../../helpers/test-setup'
import adminHeaderPage from '../../pages/admin/admin-header-page'
import mainResourcePage from '../../pages/admin/main-resource-page'
import buyerDetailsPage from '../../pages/admin/buyer-details-page'
import {
	deleteBuyerWithName,
	createDefaultBuyer,
	deleteBuyer,
} from '../../api-utils.ts/buyer-util'
import minorResourcePage from '../../pages/admin/minor-resource-page'
import catalogDetailsPage from '../../pages/admin/catalog-details-page'
import {
	deleteCatalogWithName,
	createDefaultCatalog,
	deleteCatalog,
} from '../../api-utils.ts/catalog-util'
import locationDetailsPage from '../../pages/admin/location-details-page'
import {
	deleteBuyerLocationWithName,
	createDefaultBuyerLocation,
	deleteBuyerLocation,
} from '../../api-utils.ts/buyer-locations-util'
import userDetailsPage from '../../pages/admin/user-details-page'
import { getUserID, deleteUser, createUser } from '../../api-utils.ts/users-util'
import { refreshPage } from '../../helpers/page-helper'
import { delay } from '../../helpers/wait-helper'
import { createRegExp } from '../../helpers/regExp-helper'

fixture`buyer Tests`
	.meta('TestRun', 'HS')
	.before(async ctx => {
		ctx.clientAuth = await adminClientSetup()
		ctx.buyerID = await createDefaultBuyer(ctx.clientAuth)
		const createdCatalog = await createDefaultCatalog(
			ctx.buyerID,
			ctx.clientAuth
		)
		ctx.catalogID = createdCatalog.ID
		ctx.catalogName = createdCatalog.Name
		const createdLocation = await createDefaultBuyerLocation(
			ctx.buyerID,
			ctx.clientAuth
		)
		ctx.locationID = createdLocation.Address.ID
		ctx.locationName = createdLocation.Address.AddressName

	})
	.beforeEach(async t => {
		await adminTestSetup()
	})
	.after(async ctx => {
		await deleteBuyerLocation(ctx.buyerID, ctx.locationID, ctx.clientAuth)
		await deleteCatalog(ctx.catalogID, ctx.buyerID, ctx.clientAuth)
		await deleteBuyer(ctx.buyerID, ctx.clientAuth)
	})
	.page(testConfig.adminAppUrl)

test.before(async t => {
	await adminTestSetup()

})
	.after(async t => {
		await deleteBuyerWithName(t.ctx.buyerName, t.fixtureCtx.clientAuth)
	})('Create buyer | 19971', async t => {
		await adminHeaderPage.selectAllbuyers()
		await mainResourcePage.clickCreateButton()
		const buyerName = await buyerDetailsPage.createDefaultbuyer()
		t.ctx.buyerName = buyerName
		await t.expect(await mainResourcePage.resourceExists(buyerName)).ok()
	})

//Catalog not being shown in UI after create, new to reload page to see
//https://four51.atlassian.net/browse/SEB-725
//adding in the manual wait command of t.wait(5000) circumvents intermittent failures on the product page
test.after(async t => {
	await deleteCatalogWithName(
		t.ctx.createdCatalogName,
		t.fixtureCtx.buyerID,
		t.fixtureCtx.clientAuth
	)
})('Create buyer Catalog | 19972', async t => {
	await adminHeaderPage.selectbuyerCatalogs()
	await minorResourcePage.selectParentResourceDropdown(t.fixtureCtx.buyerID)
	await minorResourcePage.clickCreateButton()
	const createdCatalogName = await catalogDetailsPage.createDefaultCatalog()
	t.ctx.createdCatalogName = createdCatalogName
	await t.wait(5000)
	await refreshPage() //refresh because of bug https://four51.atlassian.net/browse/SEB-725
	await t.wait(3000)
	await t
		.expect(await minorResourcePage.resourceExists(createdCatalogName))
		.ok('buyer Catalog not found in resource list')
})

test
	.after(async t => {
		await deleteBuyerLocationWithName(
			t.ctx.createdLocationName,
			t.fixtureCtx.buyerID,
			t.fixtureCtx.clientAuth
		)
	})('Create buyer Location | 19973', async t => {
		await adminHeaderPage.selectbuyerLocations()
		await minorResourcePage.selectParentResourceDropdown(t.fixtureCtx.buyerID)
		await t.wait(500)
		await minorResourcePage.clickCreateButton()
		const createdLocationName = await locationDetailsPage.createDefaultLocation()
		t.ctx.createdLocationName = createdLocationName
		await t
			.expect(await minorResourcePage.resourceExists(createdLocationName))
			.ok('buyer Location not found in resource list')
	})

test('Assign buyer Location to buyer Catalog | 19974', async t => {
	await adminHeaderPage.selectbuyerLocations()
	await minorResourcePage.selectParentResourceDropdown(t.fixtureCtx.buyerID)
	await minorResourcePage.clickResource(t.fixtureCtx.locationID)
	await locationDetailsPage.assignCatalogToLocation(t.fixtureCtx.catalogName)
})

test.before(async t => {
	await adminTestSetup()
})
	.after(async t => {
		const createdUserID = await getUserID(
			t.ctx.createdUserEmail,
			t.fixtureCtx.buyerID,
			t.fixtureCtx.clientAuth
		)
		await deleteUser(
			createdUserID,
			t.fixtureCtx.buyerID,
			t.fixtureCtx.clientAuth
		)
	})('Create And Assign buyer User To Location | 19975', async t => {
		await adminHeaderPage.selectbuyerUsers()
		await minorResourcePage.selectParentResourceDropdown(t.fixtureCtx.buyerID)
		await minorResourcePage.clickCreateButton()
		const createdUserEmail = await userDetailsPage.createDefaultbuyerUserWithLocation(
			t.fixtureCtx.locationName
		)
		t.ctx.createdUserEmail = createdUserEmail
		await t.expect(await minorResourcePage.resourceExists(createdUserEmail)).ok()

	})
