// import { ClientFunction } from 'testcafe'
// import testConfig from '../../testConfig'
// import {
//     adminClientSetup,
//     buyerTestSetup,
//     baseTestCleanup,
// } from '../../helpers/test-setup'
// import buyerHeaderPage from '../../pages/buyer/buyer-header-page'
// import productListPage from '../../pages/buyer/product-list-page'
// import productDetailPage from '../../pages/buyer/product-detail-page'
// import shoppingCartPage from '../../pages/buyer/shopping-cart-page'
// import checkoutPage from '../../pages/buyer/checkout-page'
// import loadingHelper from '../../helpers/loading-helper'
// import orderDetailPage from '../../pages/buyer/order-detail-page'
// import requestQuoteForm from '../../pages/buyer/request-quote-form'
// import { createDefaultBuyer } from '../../api-utils.ts/buyer-util'
// import { createDefaultCatalog } from '../../api-utils.ts/catalog-util'
// import { createDefaultBuyerLocation } from '../../api-utils.ts/buyer-locations-util'
// import randomString from '../../helpers/random-string'

// const getLocation = ClientFunction(() => document.location.href)

// fixture`Anonymous Checkout Tests`
//     .meta('TestRun', 'HS')
//     .before(async ctx => {
//         ctx.adminClientAuth = await adminClientSetup()
//     })
//     .beforeEach(async t => {
//         await t.maximizeWindow()
//         await loadingHelper.waitForLoadingBar()
//     })
//     .page(testConfig.buyerAppUrl)

// test('(Anonymous) Can I checkout with 1 item? | 2473', async t => {
//     const productName = 'Earrings'
//     const firstName = randomString(9)
//     const lastName = randomString(2)
//     await buyerHeaderPage.search(productName)
//     await productListPage.clickProduct(productName)
//     await productDetailPage.clickAddToCartButton()
//     await buyerHeaderPage.clickCartButton()
//     await shoppingCartPage.clickCheckoutButton()
//     await t.wait(3000)
//     await checkoutPage.clickCheckoutAsGuestButton()
//     await checkoutPage.enterDefaultAddress(
//         firstName, lastName
//     )
//     await checkoutPage.clickSaveAndContinueButton()
//     await checkoutPage.selectShippingOption(productName, 'day')
//     await checkoutPage.clickSaveAndContinueButton()
//     await checkoutPage.addCreditCardDuringCheckout()
//     await checkoutPage.enterCVV('112')
//     await checkoutPage.clickSaveAndContinueButton()
//     await checkoutPage.clickSubmitOrderButton()
//     await loadingHelper.thisWait()
//     await t.expect(await orderDetailPage.productExists(productName)).ok()
// })

// test('(Anonymous) Can I checkout with multiple items in my cart? | 2475', async t => {
//     const firstProductName = 'Earrings'
//     const secondProductName = 'Neckerchief'
//     const firstName = randomString(9)
//     const lastName = randomString(2)
//     await buyerHeaderPage.search(firstProductName)
//     await productListPage.clickProduct(firstProductName)
//     await productDetailPage.clickAddToCartButton()
//     await buyerHeaderPage.clearSearchText()
//     await buyerHeaderPage.search(secondProductName)
//     await productListPage.clickProduct(secondProductName)
//     await productDetailPage.clickAddToCartButton()
//     await buyerHeaderPage.clickCartButton()
//     await shoppingCartPage.clickCheckoutButton()
//     await t.wait(5000)
//     await checkoutPage.clickCheckoutAsGuestButton()
//     await t.wait(500)
//     await checkoutPage.enterDefaultAddress(
//         firstName, lastName
//     )
//     await checkoutPage.clickSaveAndContinueButton()
//     await checkoutPage.selectShippingOption(firstProductName, 'day')
//     await checkoutPage.clickSaveAndContinueButton()
//     await checkoutPage.addCreditCardDuringCheckout()
//     await checkoutPage.enterCVV('112')
//     await checkoutPage.clickSaveAndContinueButton()
//     await checkoutPage.clickSubmitOrderButton()
//     await loadingHelper.thisWait()
//     await t.expect(await orderDetailPage.productExists(firstProductName)).ok()
//     await t.expect(await orderDetailPage.productExists(secondProductName)).ok()
// })

// test('(Anonymous) Can I checkout with all items being shipped from different locations? | 2477', async t => {
//     const firstProductName = 'Earrings'
//     const secondProductName = 'Neckerchief'
//     const firstName = randomString(9)
//     const lastName = randomString(2)
//     await buyerHeaderPage.search(firstProductName)
//     await productListPage.clickProduct(firstProductName)
//     await productDetailPage.clickAddToCartButton()
//     await buyerHeaderPage.clearSearchText()
//     await buyerHeaderPage.search(secondProductName)
//     await productListPage.clickProduct(secondProductName)
//     await productDetailPage.clickAddToCartButton()
//     await buyerHeaderPage.clickCartButton()
//     await shoppingCartPage.clickCheckoutButton()
//     await t.wait(5000)
//     await checkoutPage.clickCheckoutAsGuestButton()
//     await t.wait(500)
//     await checkoutPage.enterDefaultAddress(
//         firstName, lastName
//     )
//     await checkoutPage.clickSaveAndContinueButton()
//     await checkoutPage.selectShippingOption(firstProductName, 'day')
//     await checkoutPage.selectShippingOption(secondProductName, 'day')
//     await checkoutPage.clickSaveAndContinueButton()
//     await checkoutPage.addCreditCardDuringCheckout()
//     await checkoutPage.enterCVV('900')
//     await checkoutPage.clickSaveAndContinueButton()
//     await checkoutPage.clickSubmitOrderButton()
//     await loadingHelper.thisWait()
//     await t.expect(await orderDetailPage.productExists(firstProductName)).ok()
//     await t.expect(await orderDetailPage.productExists(secondProductName)).ok()
// })

// test('(Anonymous) Can I request a quote product? | 19979', async t => {
//     const productName = 'Rock Three'
//     await buyerHeaderPage.search(productName)
//     await productListPage.clickProduct(productName)
//     await productDetailPage.clickRequestQuoteButton()
//     await requestQuoteForm.enterPhoneNumber('1231231234')
//     await requestQuoteForm.clickSubmitForQuoteButton()
//     await productDetailPage.clickViewQuoteRequestButton()
//     await t.expect(await orderDetailPage.productExists(productName)).ok()
// })
