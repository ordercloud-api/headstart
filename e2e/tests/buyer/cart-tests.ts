import { ClientFunction } from 'testcafe'
import testConfig from '../../testConfig'
import {
	adminClientSetup,
	buyerTestSetup,
	baseTestCleanup,
	existingBuyerTestSetup,
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
	.meta('TestRun', 'HS')
	.before(async ctx => {
		ctx.adminClientAuth = await adminClientSetup()
	})
	.beforeEach(async t => {
		t.ctx.testUser = await existingBuyerTestSetup(`${testConfig.buyerUsername}`, testConfig.BuyerPassword)


	})
	.page(testConfig.buyerAppUrl)

test("Can an item be added and removed from the User's cart? | 2455", async t => {
	const productName = 'Earrings'
	await buyerHeaderPage.search(productName)
	await productListPage.clickProduct(productName)
	await productDetailPage.clickAddToCartButton()
	await buyerHeaderPage.clickCartButton()
	//assert that the product is in the cart page
	await t
		.expect(
			shoppingCartPage.productsName.withText(createRegExp(productName)).exists
		)
		.ok()
	//assert that one product is there
	await t.expect(shoppingCartPage.productsName.count).eql(1)
	//remove product
	await shoppingCartPage.removeProduct()
	//assert that the product is in the cart page
	await t
		.expect(
			shoppingCartPage.products.withText(createRegExp(productName)).exists
		)
		.notOk()
	//assert that one product is there
	await t.expect(shoppingCartPage.products.count).eql(0)
})
