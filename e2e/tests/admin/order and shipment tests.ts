// import { t } from 'testcafe'
// import { userClientAuth, supplierUserRoles } from '../../api-utils.ts/auth-util'
// import {
// 	createDefaultBuyerLocation,
// 	deleteBuyerLocation,
// } from '../../api-utils.ts/buyer-locations-util'
// import { createDefaultBuyer, deleteBuyer } from '../../api-utils.ts/buyer-util'
// import {
// 	createDefaultCatalog,
// 	deleteCatalog,
// 	saveCatalogProductAssignment,
// 	setCatalogtoLocationAssignment,
// } from '../../api-utils.ts/catalog-util'
// import { createCreditCard } from '../../api-utils.ts/credit-card-util'
// import { meGetFirstAddressID } from '../../api-utils.ts/me-util'
// import {
// 	addAddresses,
// 	addLineItem,
// 	calculateCost,
// 	createOrder,
// 	deleteOrder,
// 	orderSubmit,
// 	postEstimateShipping,
// 	shipMethods,
// } from '../../api-utils.ts/order-util'
// import { updatePayment } from '../../api-utils.ts/payments-util'
// import {
// 	createDefaultProduct,
// 	deleteProduct,
// 	getProductID,
// 	saveProductAssignment,
// } from '../../api-utils.ts/product-util'
// import {
// 	createDefaultSupplierUser,
// 	getSupplierUser,
// 	deleteSupplierUser,
// } from '../../api-utils.ts/supplier-users-util'
// import {
// 	createSupplier,
// 	deleteSupplier,
// } from '../../api-utils.ts/supplier-util'
// import {
// 	createDefaultSupplierAddress,
// 	deleteSupplierAddress,
// } from '../../api-utils.ts/warehouse-util'
// import loadingHelper from '../../helpers/loading-helper'
// import { refreshPage } from '../../helpers/page-helper'
// import {
// 	adminClientSetup,
// 	adminTestSetup,
// 	baseTestCleanup,
// 	buyerTestSetup,
// 	existingBuyerTestSetup,
// 	supplierTestSetup,
// } from '../../helpers/test-setup'
// import { delay } from '../../helpers/wait-helper'
// import adminHeaderPage from '../../pages/admin/admin-header-page'
// import mainResourcePage from '../../pages/admin/main-resource-page'
// import productDetailsPage from '../../pages/admin/product-details-page'
// import buyerHeaderPage from '../../pages/buyer/buyer-header-page'
// import checkoutPage from '../../pages/buyer/checkout-page'
// import orderDetailPage from '../../pages/buyer/order-detail-page'
// import productDetailPage from '../../pages/buyer/product-detail-page'
// import productListPage from '../../pages/buyer/product-list-page'
// import shoppingCartPage from '../../pages/buyer/shopping-cart-page'
// import loginPage from '../../pages/login-page'
// import testConfig from '../../testConfig'
// import supplierOrdersPage from '../../pages/admin/supplier-orders-page'
// import { TEST_PASSWORD } from '../../test-constants'
// import { HSCatalogAssignmentRequest } from '@ordercloud/headstart-sdk'

// fixture`Order and Shipment Tests`
// 	.meta('TestRun', 'HS')
// 	.before(async ctx => {
// 		ctx.clientAuth = await adminClientSetup()
// 		ctx.supplierUserAuth = await userClientAuth(
// 			testConfig.adminAppClientID,
// 			testConfig.adminSupplierUsername,
// 			testConfig.adminSupplierPassword,
// 			supplierUserRoles
// 		)
// 		ctx.productID = await createDefaultProduct(
// 			ctx.warehouseID,
// 			ctx.supplierUserAuth
// 		)
// 		await delay(30000)
// 		await saveCatalogProductAssignment('0001', ctx.productID, ctx.clientAuth)
// 		await saveProductAssignment(
// 			'0001',
// 			ctx.productID,
// 			'JSYILMZLsU-q-meiGTsIBg',
// 			ctx.clientAuth
// 		)
// 	})
// 	.after(async ctx => {
// 		await deleteProduct(ctx.productID, ctx.supplierUserAuth)

// 	})
// 	.page(testConfig.adminAppUrl)

// test
// 	.before(async t => {
// 		const supplierUser = await getSupplierUser(
// 			testConfig.adminSupplierUserID,
// 			testConfig.adminSupplierID,
// 			t.fixtureCtx.clientAuth
// 		)
// 		t.ctx.supplierUser = supplierUser
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
// 			await deleteOrder(
// 				t.fixtureCtx.clientAuth,
// 				'Incoming',
// 				t.ctx.orderIDDelete
// 			)
// 			await baseTestCleanup(
// 				t.ctx.testUser.ID,
// 				t.fixtureCtx.buyerID,
// 				t.fixtureCtx.clientAuth
// 			)
// 		}
// 	})('Can a Product go through a full life cycle? | 20101', async t => {
// 		// Below line for Buyer side of task
// 		const buyerUser = await existingBuyerTestSetup(`${testConfig.buyerUsername}12`, testConfig.BuyerPassword)
// 		const creditCardID = await createCreditCard(t.ctx.userAuth)
// 		t.ctx.testUser = buyerUser
// 		const shippingAddressID = await meGetFirstAddressID(t.ctx.userAuth)
// 		const orderID = await createOrder(t.ctx.userAuth, 'Outgoing')
// 		const productID = t.fixtureCtx.productID
// 		await addLineItem(orderID, productID, t.ctx.userAuth, 1)
// 		await addAddresses(orderID, shippingAddressID, t.ctx.userAuth, 'Outgoing')
// 		const shippingSelection = await postEstimateShipping(
// 			orderID,
// 			t.ctx.userAuth,
// 			'Outgoing'
// 		)
// 		const shipMethodID =
// 			shippingSelection.ShipEstimateResponse.ShipEstimates[0].ShipMethods[0].ID
// 		const shipEstimateID =
// 			shippingSelection.ShipEstimateResponse.ShipEstimates[0].ID
// 		await shipMethods(
// 			shipEstimateID,
// 			shipMethodID,
// 			'Outgoing',
// 			orderID,
// 			t.ctx.userAuth
// 		)
// 		await calculateCost(orderID, t.ctx.userAuth, 'Outgoing')
// 		const paymentID = await updatePayment(t.ctx.userAuth, creditCardID, orderID)
// 		const newOrderID = await orderSubmit(
// 			orderID,
// 			paymentID,
// 			creditCardID,
// 			'USD',
// 			'112',
// 			t.ctx.userAuth
// 		)

// 		await t.navigateTo(testConfig.adminAppUrl)

// 		await adminHeaderPage.selectSalesOrdersLink()
// 		await supplierOrdersPage.searchSelectOrder(newOrderID)
// 		await supplierOrdersPage.createShipment(newOrderID)
// 		t.ctx.orderIDDelete = newOrderID
// 	})
