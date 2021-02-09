import { t } from 'testcafe'
import testConfig from '../../testConfig'
import {
	adminTestSetup,
	adminClientSetup,
	vendorTestSetup,
	buyerTestSetup,
	baseTestCleanup,
} from '../../helpers/test-setup'
import {
	createSupplier,
	deleteSupplier,
} from '../../api-utils.ts/supplier-util'
import {
	createDefaultSupplierAddress,
	deleteSupplierAddress,
} from '../../api-utils.ts/warehouse-util'
import {
	createDefaultSupplierUser,
	getSupplierUser,
	deleteSupplierUser,
	createDefaultSupplierUserWithoutRoles,
} from '../../api-utils.ts/supplier-users-util'
import adminHeaderPage from '../../pages/admin/admin-header-page'
import mainResourcePage from '../../pages/admin/main-resource-page'
import productDetailsPage from '../../pages/admin/product-details-page'
import {
	getProductID,
	deleteProduct,
	createDefaultProduct,
} from '../../api-utils.ts/product-util'
import { delay } from '../../helpers/wait-helper'
import { createDefaultBuyer, deleteBuyer } from '../../api-utils.ts/buyer-util'
import {
	createDefaultCatalog,
	deleteCatalog,
} from '../../api-utils.ts/catalog-util'
import {
	createDefaultBuyerLocation,
	deleteBuyerLocation,
} from '../../api-utils.ts/buyer-locations-util'
import {
	authBuyerBrowser,
	userClientAuth,
	vendorUserRoles,
} from '../../api-utils.ts/auth-util'
import headerPage from '../../pages/buyer/buyer-header-page'
import { scrollIntoView } from '../../helpers/element-helper'
import { refreshPage } from '../../helpers/page-helper'
import loadingHelper from '../../helpers/loading-helper'
import * as OrderCloudSDK from 'ordercloud-javascript-sdk'
import { createUser } from '../../api-utils.ts/users-util'
import buyerHeaderPage from '../../pages/buyer/buyer-header-page'
import checkoutPage from '../../pages/buyer/checkout-page'
import orderDetailPage from '../../pages/buyer/order-detail-page'
import productDetailPage from '../../pages/buyer/product-detail-page'
import productListPage from '../../pages/buyer/product-list-page'
import shoppingCartPage from '../../pages/buyer/shopping-cart-page'
import { createCreditCard } from '../../api-utils.ts/credit-card-util'

fixture`Product Tests`
	.meta('TestRun', '1')
	.before(async ctx => {
		ctx.clientAuth = await adminClientSetup()
		ctx.supplierID = await createSupplier(ctx.clientAuth)
		ctx.supplierUserID = await createDefaultSupplierUser(
			ctx.supplierID,
			ctx.clientAuth
		)
		ctx.warehouseID = await createDefaultSupplierAddress(
			ctx.supplierID,
			ctx.clientAuth
		)
		const supplierUser = await getSupplierUser(
			ctx.supplierUserID,
			ctx.supplierID,
			ctx.clientAuth
		)
		ctx.supplierUserAuth = await userClientAuth(
			testConfig.adminAppClientID,
			supplierUser.Username,
			'Test123!',
			vendorUserRoles
		)
		ctx.productID = await createDefaultProduct(
			ctx.warehouseID,
			ctx.supplierUserAuth
		)
		ctx.buyerID = await createDefaultBuyer(ctx.clientAuth)
		const catalog = await createDefaultCatalog(ctx.buyerID, ctx.clientAuth)
		ctx.catalogID = catalog.ID
		const location = await createDefaultBuyerLocation(
			ctx.buyerID,
			ctx.clientAuth
		)
		ctx.locationID = location.Address.ID
		//wait 30 seconds to let everything get setup
		await delay(30000)
	})
	.after(async ctx => {
		await deleteProduct(ctx.productID, ctx.supplierUserAuth)
		await deleteCatalog(ctx.catalogID, ctx.buyerID, ctx.clientAuth)
		await deleteBuyerLocation(ctx.buyerID, ctx.locationID, ctx.clientAuth)
		await deleteBuyer(ctx.buyerID, ctx.clientAuth)
		await deleteSupplierAddress(
			ctx.warehouseID,
			ctx.supplierID,
			ctx.clientAuth
		)
		await deleteSupplierUser(
			ctx.supplierUserID,
			ctx.supplierID,
			ctx.clientAuth
		)
		await deleteSupplier(ctx.supplierID, ctx.clientAuth)
	})
	.page(testConfig.adminAppUrl)

//Product not being shown in UI after create, new to reload page to see
//https://four51.atlassian.net/browse/SEB-725
//added work around to refresh page after creating product for all create product tests
test
	.before(async t => {
		const vendorUser = await getSupplierUser(
			t.fixtureCtx.supplierUserID,
			t.fixtureCtx.supplierID,
			t.fixtureCtx.clientAuth
		)
		await vendorTestSetup(vendorUser.Username, 'Test123!')
	})
	.after(async () => {
		if (t.ctx.createdProductName != null) {
			const createdProductID = await getProductID(
				t.ctx.createdProductName,
				t.fixtureCtx.clientAuth
			)
			await t.wait(3000)
			await deleteProduct(createdProductID, t.fixtureCtx.supplierUserAuth)
		}
	})('Create Standard Product | 19215', async t => {
	await adminHeaderPage.selectAllProducts()
	await mainResourcePage.clickCreateNewStandardProduct()
	const createdProductName = await productDetailsPage.createDefaultStandardProduct()
	t.ctx.createdProductName = createdProductName
	await t.wait(5000)
	await refreshPage() //refresh because of bug
	await t.wait(3000)
	await t
		.expect(await mainResourcePage.resourceExists(createdProductName))
		.ok()
})

test
	.before(async t => {
		const vendorUser = await getSupplierUser(
			t.fixtureCtx.supplierUserID,
			t.fixtureCtx.supplierID,
			t.fixtureCtx.clientAuth
		)
		await vendorTestSetup(vendorUser.Username, 'Test123!')
	})
	.after(async () => {
		if (t.ctx.createdProductName != null) {
			const createdProductID = await getProductID(
				t.ctx.createdProductName,
				t.fixtureCtx.clientAuth
			)
			await t.wait(3000)
			await deleteProduct(createdProductID, t.ctx.userAuth)
		}
	})('Create Quote Product | 19690', async t => {
	await adminHeaderPage.selectAllProducts()
	await mainResourcePage.clickCreateNewQuoteProduct()
	const createdProductName = await productDetailsPage.createDefaultQuoteProduct()
	t.ctx.createdProductName = createdProductName
	await t.wait(5000)
	await refreshPage() //refresh because of bug
	await t.wait(3000)
	await t
		.expect(await mainResourcePage.resourceExists(createdProductName))
		.ok()
})

test
	.before(async t => {
		const vendorUser = await getSupplierUser(
			t.fixtureCtx.supplierUserID,
			t.fixtureCtx.supplierID,
			t.fixtureCtx.clientAuth
		)
		await vendorTestSetup(vendorUser.Username, 'Test123!')
	})
	.after(async () => {
		if (t.ctx.createdProductName != null) {
			const createdProductID = await getProductID(
				t.ctx.createdProductName,
				t.fixtureCtx.clientAuth
			)
			await t.wait(3000)
			await deleteProduct(createdProductID, t.ctx.userAuth)
		}
	})('Create Purchase Order Product | 19691', async t => {
	await adminHeaderPage.selectAllProducts()
	await mainResourcePage.clickCreateNewPurchaseOrderProduct()
	const createdProductName = await productDetailsPage.createDefaultStandardProduct()
	t.ctx.createdProductName = createdProductName
	await t.wait(5000)
	await refreshPage() //refresh because of bug
	await t.wait(3000)
	await t
		.expect(await mainResourcePage.resourceExists(createdProductName))
		.ok()
})

test.before(async () => {
	await adminTestSetup()
})('Assign Product to Catalog | 19976', async t => {
	await adminHeaderPage.selectAllProducts()
	await mainResourcePage.searchForResource(t.fixtureCtx.productID)
	await mainResourcePage.selectResource(t.fixtureCtx.productID)
	await productDetailsPage.clickBuyerVisibilityTab()
	await productDetailsPage.editBuyerVisibility(t.fixtureCtx.buyerID)
})

//Check that new product shows up on buyer side
test
	.before(async t => {
		const vendorUser = await getSupplierUser(
			t.fixtureCtx.supplierUserID,
			t.fixtureCtx.supplierID,
			t.fixtureCtx.clientAuth
		)
		await vendorTestSetup(vendorUser.Username, 'Test123!')
	})
	.after(async () => {
		if (t.ctx.createdProductName != null) {
			const createdProductID = await getProductID(
				t.ctx.createdProductName,
				t.fixtureCtx.clientAuth
			)
			await t.wait(3000)
			await deleteProduct(createdProductID, t.fixtureCtx.supplierUserAuth)
			await baseTestCleanup(
				t.ctx.testUser.ID,
				'0005',
				t.fixtureCtx.clientAuth
			)
		}
	})('Can product be Created and checked out with? | 20036', async t => {
	await adminHeaderPage.selectAllProducts()
	await mainResourcePage.clickCreateNewStandardProduct()
	const createdProductName = await productDetailsPage.createDefaultActiveStandardProduct()
	t.ctx.createdProductName = createdProductName
	await t.wait(5000)
	await refreshPage() //refresh because of bug
	await t.wait(3000)
	await t
		.expect(await mainResourcePage.resourceExists(createdProductName))
		.ok()
	await adminHeaderPage.logout()

	//Below for Product visibility
	await adminTestSetup()

	await adminHeaderPage.selectAllProducts()
	await mainResourcePage.searchForResource(createdProductName)
	await mainResourcePage.selectResource(createdProductName)
	await productDetailsPage.clickacceptChangesButton()
	await productDetailsPage.clickBuyerVisibilityTab()
	await productDetailsPage.editBuyerVisibilityForView(
		'0005',
		'All Location Products'
	)

	// Below line for Buyer side of task
	await t.navigateTo(testConfig.buyerAppUrl)
	const buyerUser = await buyerTestSetup(t.fixtureCtx.clientAuth)
	t.ctx.testUser = buyerUser

	await buyerHeaderPage.search(createdProductName)
	await productListPage.clickProduct(createdProductName)
	await productDetailPage.clickAddToCartButton()
	await buyerHeaderPage.clickCartButton()
	await shoppingCartPage.clickCheckoutButton()
	await checkoutPage.clickSaveAndContinueButton()
	await checkoutPage.selectShippingOption(createdProductName, 'day')
	await checkoutPage.clickSaveAndContinueButton()
	await checkoutPage.selectCreditCard(buyerUser.FirstName)
	await checkoutPage.enterCVV('900')
	await checkoutPage.clickSaveAndContinueButton()
	await checkoutPage.clickSubmitOrderButton()
	await loadingHelper.thisWait()
	await t.expect(await orderDetailPage.productExists(createdProductName)).ok()
})
