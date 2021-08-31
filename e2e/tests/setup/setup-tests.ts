import {
	adminClientSetup,
	adminTestSetup,
	supplierTestSetup,
} from '../../helpers/test-setup'
import testConfig from '../../testConfig'
import adminHeaderPage from '../../pages/admin/admin-header-page'
import mainResourcePage from '../../pages/admin/main-resource-page'
import supplierDetailsPage from '../../pages/admin/supplier-details-page'
import minorResourcePage from '../../pages/admin/minor-resource-page'
import userDetailsPage from '../../pages/admin/user-details-page'
import { setupGetSupplierID } from '../../api-utils.ts/supplier-util'
import {
	setupGetSupplierUserID,
	updateSupplierUser,
} from '../../api-utils.ts/supplier-users-util'
import productDetailsPage from '../../pages/admin/product-details-page'
import warehouseDetailsPage from '../../pages/admin/warehouse-details-page'
import { TEST_PASSWORD } from '../../test-constants'

fixture`Setup Tests`
	.meta('TestRun', 'Setup')
	.before(async ctx => {
		ctx.clientAuth = await adminClientSetup()
	})
	.beforeEach(async t => {
		await adminTestSetup()
	})
	.page(testConfig.adminAppUrl)

test('Setup Create Supplier', async t => {
	await adminHeaderPage.selectAllSuppliers()
	await mainResourcePage.clickCreateButton()
	await supplierDetailsPage.createSupplier(
		true,
		"Speedwagon's Foundation",
		'united states dollar',
		['Standard', 'Quote', 'Purchase Order', 'Tester'],
		'Linens',
		'mandated'
	)
})

test('Setup Create Supplier User', async t => {
	await adminHeaderPage.selectSupplierUsers()
	await minorResourcePage.selectParentResourceDropdown(
		"Speedwagon's Foundation"
	)
	await minorResourcePage.clickCreateButton()
	await userDetailsPage.createSupplierUser(
		'robertspeedwagon@fakeemail123.com',
		'robertspeedwagon',
		'robert',
		'speedwagon'
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
		{ Password: TEST_PASSWORD },
		t.fixtureCtx.clientAuth
	)
})

test('Setup Supplier Warehouse', async t => {
	await adminHeaderPage.selectSupplierWarehouses()
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
	await supplierTestSetup('robertspeedwagon', 'Bigwords123!')
})('Setup Product', async t => {
	await adminHeaderPage.selectAllProducts()
	await mainResourcePage.clickCreateNewStandardProduct()
	await productDetailsPage.createProduct('Better Product', 'Better Warehouse')
})

//create brand user
