import { t } from 'testcafe'
import testConfig from '../../testConfig'
import { adminTestSetup, adminClientSetup } from '../../helpers/test-setup'
import adminHeaderPage from '../../pages/admin/admin-header-page'
import { createSupplier } from '../../api-utils.ts/supplier-util'
import {
	getWarehouseID,
	deleteSupplierAddress,
} from '../../api-utils.ts/warehouse-util'
import mainResourcePage from '../../pages/admin/main-resource-page'
import supplierDetailsPage from '../../pages/admin/supplier-details-page'
import minorResourcePage from '../../pages/admin/minor-resource-page'
import userDetailsPage from '../../pages/admin/user-details-page'
import warehouseDetailsPage from '../../pages/admin/warehouse-details-page'
import {
	getSupplierUserID,
	deleteSupplierUser,
	createDefaultSupplierUser,
	createDefaultSupplierUserWithoutRoles,
} from '../../api-utils.ts/supplier-users-util'
import {
	cleanupSupplierWithName,
	cleanupSupplierWithID,
} from '../../helpers/test-cleanup'
import { delay } from '../../helpers/wait-helper'

fixture`Supplier Tests`
	.meta('TestRun', 'HS')
	.before(async ctx => {
		ctx.clientAuth = await adminClientSetup()
		ctx.supplierID = await createSupplier(ctx.clientAuth)
		ctx.supplierUserID = await createDefaultSupplierUserWithoutRoles(
			ctx.supplierID,
			ctx.clientAuth
		)
		//wait 5 seconds to let everything get setup
		await delay(5000)
	})
	.beforeEach(async t => {
		await adminTestSetup()
	})
	.after(async ctx => {
		await deleteSupplierUser(
			ctx.supplierUserID,
			ctx.supplierID,
			ctx.clientAuth
		)
		await cleanupSupplierWithID(ctx.supplierID, ctx.clientAuth)
	})
	.page(testConfig.adminAppUrl)

test.after(async t => {
	await cleanupSupplierWithName(t.ctx.supplierName, t.fixtureCtx.clientAuth)
})('Create Supplier | 19967', async t => {
	await adminHeaderPage.selectAllSuppliers()
	await mainResourcePage.clickCreateButton()
	const supplierName = await supplierDetailsPage.createDefaultSupplier()
	t.ctx.supplierName = supplierName
	await t.expect(await mainResourcePage.resourceExists(supplierName)).ok()
})

test.after(async t => {
	const createdUserID = await getSupplierUserID(
		t.ctx.createdUserEmail,
		t.fixtureCtx.supplierID,
		t.fixtureCtx.clientAuth
	)
	await deleteSupplierUser(
		createdUserID,
		t.fixtureCtx.supplierID,
		t.fixtureCtx.clientAuth
	)
})('Create Supplier User | 19968', async t => {
	await adminHeaderPage.selectSupplierUsers()
	await minorResourcePage.selectParentResourceDropdown(t.fixtureCtx.supplierID)
	await minorResourcePage.clickCreateButton()
	const createdUserEmail = await userDetailsPage.createDefaultSupplierUser()
	t.ctx.createdUserEmail = createdUserEmail
	await t.expect(await minorResourcePage.resourceExists(createdUserEmail)).ok()
})

test.after(async t => {
	const warehouseID = await getWarehouseID(
		t.ctx.warehouseName,
		t.fixtureCtx.supplierID,
		t.fixtureCtx.clientAuth
	)
	await deleteSupplierAddress(
		warehouseID,
		t.fixtureCtx.supplierID,
		t.fixtureCtx.clientAuth
	)
})('Create Supplier Warehouse | 19969', async t => {
	await adminHeaderPage.selectSupplierWarehouses()
	await minorResourcePage.selectParentResourceDropdown(t.fixtureCtx.supplierID)
	await minorResourcePage.clickCreateButton()
	const warehouseName = await warehouseDetailsPage.createDefaultWarehouse()
	t.ctx.warehouseName = warehouseName
	await t.expect(await minorResourcePage.resourceExists(warehouseName)).ok()
})

//failing because of: https://four51.atlassian.net/browse/SEB-1065
test('Assign Roles to Supplier User | 19970', async t => {
	await adminHeaderPage.selectSupplierUsers()
	await minorResourcePage.selectParentResourceDropdown(t.fixtureCtx.supplierID)
	await minorResourcePage.clickResource(t.fixtureCtx.supplierUserID)
	await userDetailsPage.updateUserPermissions()
})
