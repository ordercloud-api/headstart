import { ClientFunction } from 'testcafe'
import testConfig from '../../testConfig'
import {
	adminClientSetup,
	buyerTestSetup,
	baseTestCleanup,
} from '../../helpers/test-setup'
import buyerHeaderPage from '../../pages/buyer/buyer-header-page'
import productListPage from '../../pages/buyer/product-list-page'
import productDetailPage from '../../pages/buyer/product-detail-page'
import shoppingCartPage from '../../pages/buyer/shopping-cart-page'
import checkoutPage from '../../pages/buyer/checkout-page'
import loadingHelper from '../../helpers/loading-helper'
import orderDetailPage from '../../pages/buyer/order-detail-page'
import requestQuoteForm from '../../pages/buyer/request-quote-form'

const getLocation = ClientFunction(() => document.location.href)

fixture`Checkout Tests`
	.meta('TestRun', '1')
	.before(async ctx => {
		ctx.adminClientAuth = await adminClientSetup()
	})
	.beforeEach(async t => {
		t.ctx.testUser = await buyerTestSetup(t.fixtureCtx.adminClientAuth)
	})
	.afterEach(async t => {
		await baseTestCleanup(
			t.ctx.testUser.ID,
			'0005',
			t.fixtureCtx.adminClientAuth
		)
	})
	.page(testConfig.buyerAppUrl)

test('Can I checkout with 1 item? | 2473', async t => {
	const productName = '100 CLASS T-SHIRT'
	await buyerHeaderPage.search(productName)
	await productListPage.clickProduct(productName)
	await productDetailPage.clickAddToCartButton()
	await buyerHeaderPage.clickCartButton()
	await shoppingCartPage.clickCheckoutButton()
	await checkoutPage.clickSaveAndContinueButton()
	await checkoutPage.selectShippingOption(productName, 'day')
	await checkoutPage.clickSaveAndContinueButton()
	await checkoutPage.selectCreditCard(t.ctx.testUser.FirstName)
	await checkoutPage.enterCVV('900')
	await checkoutPage.clickSaveAndContinueButton()
	await checkoutPage.clickSubmitOrderButton()
	await loadingHelper.thisWait()
	await t.expect(await orderDetailPage.productExists(productName)).ok()
})

test('Can I checkout with multiple items in my cart? | 2475', async t => {
	const firstProductName = '100 CLASS T-SHIRT'
	const secondProductName = '500 CLASS HOODIE'
	await buyerHeaderPage.search(firstProductName)
	await productListPage.clickProduct(firstProductName)
	await productDetailPage.clickAddToCartButton()
	await buyerHeaderPage.clearSearchText()
	await buyerHeaderPage.search(secondProductName)
	await productListPage.clickProduct(secondProductName)
	await productDetailPage.clickAddToCartButton()
	await buyerHeaderPage.clickCartButton()
	await shoppingCartPage.clickCheckoutButton()
	await checkoutPage.clickSaveAndContinueButton()
	await checkoutPage.selectShippingOption(firstProductName, 'day')
	await checkoutPage.clickSaveAndContinueButton()
	await checkoutPage.selectCreditCard(t.ctx.testUser.FirstName)
	await checkoutPage.enterCVV('900')
	await checkoutPage.clickSaveAndContinueButton()
	await checkoutPage.clickSubmitOrderButton()
	await loadingHelper.thisWait()
	await t.expect(await orderDetailPage.productExists(firstProductName)).ok()
	await t.expect(await orderDetailPage.productExists(secondProductName)).ok()
})

test('Can I checkout with all items being shipped from different locations? | 2477', async t => {
	const firstProductName = '100 CLASS T-SHIRT'
	const secondProductName = 'Rubber Octagonal Dumbbell'
	await buyerHeaderPage.search(firstProductName)
	await productListPage.clickProduct(firstProductName)
	await productDetailPage.clickAddToCartButton()
	await buyerHeaderPage.clearSearchText()
	await buyerHeaderPage.search(secondProductName)
	await productListPage.clickProduct(secondProductName)
	await productDetailPage.clickAddToCartButton()
	await buyerHeaderPage.clickCartButton()
	await shoppingCartPage.clickCheckoutButton()
	await checkoutPage.clickSaveAndContinueButton()
	await checkoutPage.selectShippingOption(firstProductName, 'day')
	await checkoutPage.selectShippingOption(secondProductName, 'day')
	await checkoutPage.clickSaveAndContinueButton()
	await checkoutPage.selectCreditCard(t.ctx.testUser.FirstName)
	await checkoutPage.enterCVV('900')
	await checkoutPage.clickSaveAndContinueButton()
	await checkoutPage.clickSubmitOrderButton()
	await loadingHelper.thisWait()
	await t.expect(await orderDetailPage.productExists(firstProductName)).ok()
	await t.expect(await orderDetailPage.productExists(secondProductName)).ok()
})

test('Can the User add an address during checkout? | 19689', async t => {
	const productName = '100 CLASS T-SHIRT'
	await buyerHeaderPage.search(productName)
	await productListPage.clickProduct(productName)
	await productDetailPage.clickAddToCartButton()
	await buyerHeaderPage.clickCartButton()
	await shoppingCartPage.clickCheckoutButton()
	await checkoutPage.clickAddNewAddressButton()
	await checkoutPage.enterDefaultAddress(
		t.ctx.testUser.FirstName,
		t.ctx.testUser.LastName
	)
	await checkoutPage.clickSaveAndContinueButton()
})

test('Can a User checkout with a PO product in the cart? | 19977', async t => {
	const productName = 'accessfob'
	await buyerHeaderPage.search(productName)
	await productListPage.clickProduct(productName)
	await productDetailPage.clickAddToCartButton()
	await buyerHeaderPage.clickCartButton()
	await shoppingCartPage.clickCheckoutButton()
	await checkoutPage.clickSaveAndContinueButton()
	await checkoutPage.selectShippingOption(productName, 'day')
	await checkoutPage.clickSaveAndContinueButton()
	await loadingHelper.thisWait()
	await checkoutPage.clickAcceptButton()
	await checkoutPage.clickContinueButton()
	await checkoutPage.clickSubmitOrderButton()
	await loadingHelper.thisWait()
	await t.expect(await orderDetailPage.productExists(productName)).ok()
})

test('Can a user checkout with a PO product and standard product in their cart? | 19978', async t => {
	const firstProductName = '100 CLASS T-SHIRT'
	const secondProductName = 'accessfob'
	await buyerHeaderPage.search(firstProductName)
	await productListPage.clickProduct(firstProductName)
	await productDetailPage.clickAddToCartButton()
	await buyerHeaderPage.clearSearchText()
	await buyerHeaderPage.search(secondProductName)
	await productListPage.clickProduct(secondProductName)
	await productDetailPage.clickAddToCartButton()
	await buyerHeaderPage.clickCartButton()
	await shoppingCartPage.clickCheckoutButton()
	await checkoutPage.clickSaveAndContinueButton()
	await checkoutPage.selectShippingOption(firstProductName, 'day')
	await checkoutPage.selectShippingOption(secondProductName, 'day')
	await checkoutPage.clickSaveAndContinueButton()
	await checkoutPage.selectCreditCard(t.ctx.testUser.FirstName)
	await checkoutPage.enterCVV('900')
	await checkoutPage.clickAcceptButton()
	await checkoutPage.clickSaveAndContinueButton()
	await checkoutPage.clickSubmitOrderButton()
	await loadingHelper.thisWait()
	await t.expect(await orderDetailPage.productExists(firstProductName)).ok()
	await t.expect(await orderDetailPage.productExists(secondProductName)).ok()
})

//This is no longer throwing errors, adding it back in to test suite
test('Can I request a quote product? | 19979', async t => {
	const productName = '4 X 4 Foot Siege Storage Rack - X1 Package'
	await buyerHeaderPage.search(productName)
	await productListPage.clickProduct(productName)
	await productDetailPage.clickRequestQuoteButton()
	await requestQuoteForm.enterPhoneNumber('1231231234')
	await requestQuoteForm.clickSubmitForQuoteButton()
	await productDetailPage.clickViewQuoteRequestButton()
	await t.expect(await orderDetailPage.productExists(productName)).ok()
})
