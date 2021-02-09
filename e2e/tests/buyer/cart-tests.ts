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
import { createRegExp } from '../../helpers/regExp-helper'

const getLocation = ClientFunction(() => document.location.href)

fixture`Cart Tests`
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

test("Can an item be added to the User's cart? | 2455", async t => {
	const productName = '100 CLASS T-SHIRT'
	await buyerHeaderPage.search(productName)
	await productListPage.clickProduct(productName)
	await productDetailPage.clickAddToCartButton()
	await buyerHeaderPage.clickCartButton()
	//assert that the product is in the cart page
	await t
		.expect(
			shoppingCartPage.products.withText(createRegExp(productName)).exists
		)
		.ok()
	//assert that one product is there
	await t.expect(shoppingCartPage.products.count).eql(1)
})

test('Can a User remove an item from their cart? | 2427', async t => {
	const productName = '100 CLASS T-SHIRT'
	await buyerHeaderPage.search(productName)
	await productListPage.clickProduct(productName)
	await productDetailPage.clickAddToCartButton()
	await buyerHeaderPage.clickCartButton()
	//remove product
	await shoppingCartPage.removeProduct(productName)
	//assert that the product is in the cart page
	await t
		.expect(
			shoppingCartPage.products.withText(createRegExp(productName)).exists
		)
		.notOk()
	//assert that one product is there
	await t.expect(shoppingCartPage.products.count).eql(0)
})
