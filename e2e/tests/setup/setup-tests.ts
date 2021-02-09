import {
	adminClientSetup,
	adminTestSetup,
	vendorTestSetup,
} from '../../helpers/test-setup'
import testConfig from '../../testConfig'
import adminHeaderPage from '../../pages/admin/admin-header-page'
import mainResourcePage from '../../pages/admin/main-resource-page'
import vendorDetailsPage from '../../pages/admin/vendor-details-page'
import minorResourcePage from '../../pages/admin/minor-resource-page'
import userDetailsPage from '../../pages/admin/user-details-page'
import { setupGetSupplierID } from '../../api-utils.ts/supplier-util'
import {
	setupGetSupplierUserID,
	updateSupplierUser,
} from '../../api-utils.ts/supplier-users-util'
import productDetailsPage from '../../pages/admin/product-details-page'
import warehouseDetailsPage from '../../pages/admin/warehouse-details-page'

fixture`Setup Tests`
	.meta('TestRun', 'Setup')
	.before(async ctx => {
		ctx.clientAuth = await adminClientSetup()
	})
	.beforeEach(async t => {
		await adminTestSetup()
	})
	.page(testConfig.adminAppUrl)

test('Setup Create Vendor', async t => {
	await adminHeaderPage.selectAllVendors()
	await mainResourcePage.clickCreateButton()
	await vendorDetailsPage.createVendor(
		true,
		"Speedwagon's Foundation",
		'united states dollar',
		['Standard', 'Quote', 'Purchase Order'],
		'Linens',
		'mandated'
	)
})

test('Setup Create Vendor User', async t => {
	await adminHeaderPage.selectVendorUsers()
	await minorResourcePage.selectParentResourceDropdown(
		"Speedwagon's Foundation"
	)
	await minorResourcePage.clickCreateButton()
	await userDetailsPage.createVendorUser(
		'robertspeedwagon',
		'robertspeedwagon@mailinator.com',
		'Robert',
		'Speedwagon'
	)

	//Update supplier user password
	const supplierID = await setupGetSupplierID(
		"Speedwagon's Foundation",
		t.fixtureCtx.clientAuth
	)

	const userID = await setupGetSupplierUserID(
		'robertspeedwagon',
		supplierID,
		t.fixtureCtx.clientAuth
	)

	await updateSupplierUser(
		supplierID,
		userID,
		{ Password: 'fails345' },
		t.fixtureCtx.clientAuth
	)
})

test('Setup Vendor Warehouse', async t => {
	await adminHeaderPage.selectVendorWarehouses()
	await minorResourcePage.selectParentResourceDropdown(
		"Speedwagon's Foundation"
	)
	await minorResourcePage.clickCreateButton()
	await warehouseDetailsPage.createWarehouse(
		'Better Warehouse',
		'Better Company'
	)
})

test.before(async t => {
	await vendorTestSetup('robertspeedwagon', 'fails345')
})('Setup Product', async t => {
	await adminHeaderPage.selectAllProducts()
	await mainResourcePage.clickCreateNewStandardProduct()
	await productDetailsPage.createProduct('Better Product', 'Better Warehouse')
})

//create brand user
