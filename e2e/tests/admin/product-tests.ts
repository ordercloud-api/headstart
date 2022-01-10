import { t } from 'testcafe'
import testConfig from '../../testConfig'
import {
	adminTestSetup,
	adminClientSetup,
	supplierTestSetup,
	buyerTestSetup,
	baseTestCleanup,
	existingBuyerTestSetup,
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
	saveProductAssignment,
} from '../../api-utils.ts/product-util'
import { delay } from '../../helpers/wait-helper'
import { createDefaultBuyer, deleteBuyer } from '../../api-utils.ts/buyer-util'
import {
	createDefaultCatalog,
	deleteCatalog,
	saveCatalogProductAssignment,
	setCatalogtoLocationAssignment,
} from '../../api-utils.ts/catalog-util'
import {
	createDefaultBuyerLocation,
	deleteBuyerLocation,
} from '../../api-utils.ts/buyer-locations-util'
import {
	authBuyerBrowser,
	userClientAuth,
	supplierUserRoles,
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
import { TEST_PASSWORD } from '../../test-constants'
import loginPage from '../../pages/login-page'
import { HSCatalogAssignmentRequest } from '@ordercloud/headstart-sdk'

fixture`Product Tests`
	.meta('TestRun', 'HS')
	.before(async ctx => {

		ctx.clientAuth = await adminClientSetup()

		ctx.supplierUserAuth = await userClientAuth(
			testConfig.adminAppClientID,
			testConfig.adminSupplierUsername,
			testConfig.adminSupplierPassword,
			supplierUserRoles
		)
		ctx.warehouseID = await createDefaultSupplierAddress(
			testConfig.adminSupplierID,
			ctx.clientAuth
		)
		ctx.productID = await createDefaultProduct(
			ctx.warehouseID,
			ctx.supplierUserAuth
		)
		ctx.buyerID = await createDefaultBuyer(ctx.clientAuth)
		const catalog = await createDefaultCatalog(ctx.buyerID, ctx.clientAuth)
		const hSCatalogAssignment: HSCatalogAssignmentRequest = { CatalogIDs: [catalog.ID] }
		ctx.catalogID = catalog.ID
		ctx.catalogName = catalog.Name
		ctx.catalog = catalog
		const location = await createDefaultBuyerLocation(
			ctx.buyerID,
			ctx.clientAuth,
		)
		ctx.locationID = location.Address.ID
		await setCatalogtoLocationAssignment(ctx.buyerID, ctx.locationID, hSCatalogAssignment, ctx.clientAuth)
		//wait 30 seconds to let everything get setup
		await delay(30000)
		//wait 30 seconds to let everything get setup
		await delay(30000)
		ctx.productID = await createDefaultProduct(
			ctx.warehouseID,
			ctx.supplierUserAuth
		)
		await saveCatalogProductAssignment(ctx.buyerID, ctx.productID, ctx.clientAuth)
		await saveProductAssignment(
			ctx.buyerID,
			ctx.productID,
			ctx.catalogID,
			ctx.clientAuth
		)
	})
	.after(async ctx => {
		await deleteCatalog(ctx.catalogID, ctx.buyerID, ctx.clientAuth)
		await deleteBuyerLocation(ctx.buyerID, ctx.locationID, ctx.clientAuth)
		await deleteBuyer(ctx.buyerID, ctx.clientAuth)
		await deleteProduct(ctx.productID, ctx.supplierUserAuth)

	})
	.page(testConfig.adminAppUrl)

//Product not being shown in UI after create, new to reload page to see
//https://four51.atlassian.net/browse/SEB-725
//added work around to refresh page after creating product for all create product tests
// additional t.wait is implemented because a manual wait has to be used to allow 
test
	.before(async t => {
		await supplierTestSetup(testConfig.adminSupplierUsername, testConfig.adminSupplierPassword)

	})
	.after(async t => {
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
		await refreshPage() //refresh because of bug https://four51.atlassian.net/browse/SEB-725
		await t.wait(3000)
		await t
			.expect(await mainResourcePage.resourceExists(createdProductName))
			.ok()
	})

test
	.before(async t => {
		await supplierTestSetup(testConfig.adminSupplierUsername, testConfig.adminSupplierPassword)
	})
	.after(async t => {
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
		await refreshPage() //refresh because of bug https://four51.atlassian.net/browse/SEB-725
		await t.wait(3000)
		await t
			.expect(await mainResourcePage.resourceExists(createdProductName))
			.ok()
	})


test.before(async t => {
	await adminTestSetup()
})('Assign Product to Catalog | 19976', async t => {
	await adminHeaderPage.selectAllProducts()
	await mainResourcePage.searchForResource(t.fixtureCtx.productID)
	await mainResourcePage.selectResource(t.fixtureCtx.productID)
	await productDetailsPage.clickBuyerVisibilityTab()
	await productDetailsPage.editBuyerVisibility(t.fixtureCtx.buyerID)
})

//Check that new product shows up on buyer side
// test
// 	.before(async t => {
// 		await supplierTestSetup(testConfig.adminSupplierUsername, testConfig.adminSupplierPassword)

// 	})
// 	.after(async t => {
// 		if (t.ctx.createdProductName != null) {
// 			const createdProductID = await getProductID(
// 				t.ctx.createdProductName,
// 				t.fixtureCtx.clientAuth
// 			)
// 			await t.wait(3000)
// 			await deleteProduct(createdProductID, t.fixtureCtx.supplierUserAuth)
// 		}
// 	})('Can a product be Created and checked out with? | 20036', async t => {
// 		await t.wait(50000)
// 		await adminHeaderPage.selectAllProducts()
// 		await mainResourcePage.clickCreateNewStandardProduct()
// 		const createdProductName = await productDetailsPage.createDefaultActiveStandardProduct()
// 		t.ctx.createdProductName = createdProductName
// 		await t.wait(5000)
// 		await refreshPage() //refresh because of bug https://four51.atlassian.net/browse/SEB-725
// 		await t.wait(3000)
// 		await t
// 			.expect(await mainResourcePage.resourceExists(createdProductName))
// 			.ok()
// 		await adminHeaderPage.logout()

// 		//Below for Product visibility
// 		await adminTestSetup()
// 		await adminHeaderPage.selectAllProducts()
// 		await mainResourcePage.searchForResource(createdProductName)
// 		await mainResourcePage.selectResource(createdProductName)
// 		await productDetailsPage.clickBuyerVisibilityTab()
// 		await productDetailsPage.editBuyerVisibilityForView(
// 			'Default HeadStart Buyer',
// 			'BuyerLocation1'
// 		)

// 		// Below line for Buyer side of task
// 		await t.navigateTo(testConfig.buyerAppUrl)
// 		const buyerUser = existingBuyerTestSetup(`${testConfig.buyerUsername}6`, testConfig.BuyerPassword)
// 		t.ctx.testUser = buyerUser
// 		await buyerHeaderPage.search(createdProductName)
// 		await productListPage.clickProduct(createdProductName)
// 		await productDetailPage.clickAddToCartButton()
// 		await buyerHeaderPage.clickCartButton()
// 		await shoppingCartPage.clickCheckoutButton()
// 		await checkoutPage.clickSaveAndContinueButton()
// 		await checkoutPage.selectShippingOption(createdProductName, 'day')
// 		await checkoutPage.clickSaveAndContinueButton()
// 		await checkoutPage.selectCreditCard('Automated Credit Card')
// 		await checkoutPage.enterCVV('900')
// 		await checkoutPage.clickSaveAndContinueButton()
// 		await checkoutPage.clickSubmitOrderButton()
// 		await loadingHelper.thisWait()
// 		await t.expect(await orderDetailPage.productExists(createdProductName)).ok()
// 	})
