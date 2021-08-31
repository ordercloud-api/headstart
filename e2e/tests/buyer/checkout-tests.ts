import { ClientFunction } from 'testcafe'
import testConfig from '../../testConfig'
import {
    adminClientSetup,
    buyerTestSetup,
    baseTestCleanup,
    existingBuyerTestSetup,
    supplierTestSetup,
} from '../../helpers/test-setup'
import buyerHeaderPage from '../../pages/buyer/buyer-header-page'
import productListPage from '../../pages/buyer/product-list-page'
import productDetailPage from '../../pages/buyer/product-detail-page'
import shoppingCartPage from '../../pages/buyer/shopping-cart-page'
import checkoutPage from '../../pages/buyer/checkout-page'
import loadingHelper from '../../helpers/loading-helper'
import orderDetailPage from '../../pages/buyer/order-detail-page'
import requestQuoteForm from '../../pages/buyer/request-quote-form'
import { createDefaultBuyer } from '../../api-utils.ts/buyer-util'
import { createDefaultCatalog, saveCatalogProductAssignment } from '../../api-utils.ts/catalog-util'
import { createDefaultBuyerLocation } from '../../api-utils.ts/buyer-locations-util'
import randomString from '../../helpers/random-string'
import { delay } from '../../helpers/wait-helper'
import { userClientAuth, supplierUserRoles } from '../../api-utils.ts/auth-util'
import { createDefaultProduct, deleteProduct, saveProductAssignment } from '../../api-utils.ts/product-util'
import { createDefaultSupplierAddress } from '../../api-utils.ts/warehouse-util'
import adminHeaderPage from '../../pages/admin/admin-header-page'
import supplierOrdersPage from '../../pages/admin/supplier-orders-page'
import { getSupplierUser } from '../../api-utils.ts/supplier-users-util'
import loginPage from '../../pages/login-page'

const getLocation = ClientFunction(() => document.location.href)

fixture`Checkout Tests`
    .meta('TestRun', 'HS')
    .before(async ctx => {
        ctx.clientAuth = await adminClientSetup()

    })

    .page(testConfig.buyerAppUrl)

test.before(async t => {
    t.ctx.testUser = await existingBuyerTestSetup(`${testConfig.buyerUsername}2`, testConfig.BuyerPassword)
})('Can I checkout with 1 item? | 2473', async t => {
    const productName = 'Earrings'
    const firstName = randomString(9)
    const lastName = randomString(2)
    await buyerHeaderPage.search(productName)
    await productListPage.clickProduct(productName)
    await productDetailPage.clickAddToCartButton()
    await buyerHeaderPage.clickCartButton()
    await shoppingCartPage.clickCheckoutButton()
    await t.wait(500)
    await checkoutPage.clickSaveAndContinueButton()
    await checkoutPage.selectShippingOption(productName, 'day')
    await checkoutPage.clickSaveAndContinueButton()
    await checkoutPage.selectCreditCard('Automated Credit Card')
    await checkoutPage.enterCVV('112')
    await checkoutPage.clickSaveAndContinueButton()
    await checkoutPage.clickSubmitOrderButton()
    await loadingHelper.thisWait()
    await t.expect(await orderDetailPage.productExists(productName)).ok()
})

test.before(async t => {
    t.ctx.testUser = await existingBuyerTestSetup(`${testConfig.buyerUsername}17`, testConfig.BuyerPassword)
})('Can I checkout with multiple items in my cart? | 2475', async t => {
    const firstProductName = 'Earrings'
    const secondProductName = 'Neckerchief'
    const firstName = randomString(9)
    const lastName = randomString(2)
    await t.wait(10000)
    await buyerHeaderPage.search(firstProductName)
    await productListPage.clickProduct(firstProductName)
    await productDetailPage.clickAddToCartButton()
    await buyerHeaderPage.clearSearchText()
    await buyerHeaderPage.search(secondProductName)
    await productListPage.clickProduct(secondProductName)
    await productDetailPage.clickAddToCartButton()
    await buyerHeaderPage.clickCartButton()
    await shoppingCartPage.clickCheckoutButton()
    await t.wait(500)
    await checkoutPage.clickSaveAndContinueButton()
    await checkoutPage.selectShippingOption(firstProductName, 'day')
    await checkoutPage.clickSaveAndContinueButton()
    await checkoutPage.selectCreditCard('Automated Credit Card')
    await checkoutPage.enterCVV('112')
    await checkoutPage.clickSaveAndContinueButton()
    await checkoutPage.clickSubmitOrderButton()
    await loadingHelper.thisWait()
    await t.expect(await orderDetailPage.productExists(firstProductName)).ok()
    await t.expect(await orderDetailPage.productExists(secondProductName)).ok()
})

test.before(async t => {
    t.ctx.testUser = await existingBuyerTestSetup(`${testConfig.buyerUsername}18`, testConfig.BuyerPassword)
})('Can I checkout with all items being shipped from different locations? | 2477', async t => {
    const firstProductName = 'Earrings'
    const secondProductName = 'Neckerchief'
    const firstName = randomString(9)
    const lastName = randomString(2)
    await buyerHeaderPage.search(firstProductName)
    await productListPage.clickProduct(firstProductName)
    await productDetailPage.clickAddToCartButton()
    await buyerHeaderPage.clearSearchText()
    await buyerHeaderPage.search(secondProductName)
    await productListPage.clickProduct(secondProductName)
    await productDetailPage.clickAddToCartButton()
    await buyerHeaderPage.clickCartButton()
    await shoppingCartPage.clickCheckoutButton()
    await t.wait(500)
    await checkoutPage.clickSaveAndContinueButton()
    await checkoutPage.selectShippingOption(firstProductName, 'day')
    await checkoutPage.selectShippingOption(secondProductName, 'day')
    await checkoutPage.clickSaveAndContinueButton()
    await checkoutPage.selectCreditCard('Automated Credit Card')
    await checkoutPage.enterCVV('900')
    await checkoutPage.clickSaveAndContinueButton()
    await checkoutPage.clickSubmitOrderButton()
    await loadingHelper.thisWait()
    await t.expect(await orderDetailPage.productExists(firstProductName)).ok()
    await t.expect(await orderDetailPage.productExists(secondProductName)).ok()
})

test.before(async t => {
    t.ctx.testUser = await existingBuyerTestSetup(`${testConfig.buyerUsername}`, testConfig.BuyerPassword)
})('Can the User add an address during checkout? | 19689', async t => {
    const productName = 'Earrings'
    await buyerHeaderPage.search(productName)
    await productListPage.clickProduct(productName)
    await productDetailPage.clickAddToCartButton()
    await buyerHeaderPage.clickCartButton()
    await shoppingCartPage.clickCheckoutButton()
    await checkoutPage.clickAddNewAddressButton()
    await checkoutPage.enterDefaultAddress(
        'First Name',
        'Last Name',
    )
    await checkoutPage.clickSaveAndContinueButton()
})

test.before(async t => {
    t.ctx.testUser = await existingBuyerTestSetup(`${testConfig.buyerUsername}19`, testConfig.BuyerPassword)
})('Can I request a quote product? | 19979', async t => {
    const productName = 'Rock Three'
    await buyerHeaderPage.search(productName)
    await productListPage.clickProduct(productName)
    await productDetailPage.clickRequestQuoteButton()
    // await requestQuoteForm.quoteSubmitForm()
    await requestQuoteForm.clickSubmitForQuoteButton()
    await productDetailPage.clickViewQuoteRequestButton()
    await t.expect(await orderDetailPage.productExists(productName)).ok()
})




test.before(async t => {
    t.ctx.testUser = await existingBuyerTestSetup(`${testConfig.buyerUsername}3`, testConfig.BuyerPassword)


    await t.navigateTo(testConfig.buyerAppUrl)

})
    ('Can I checkout with all items being shipped from different locations (Seller Validation)? | 2477', async t => {
        const firstProductName = 'Bird Statue'
        const secondProductName = 'Human Statue'
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
        await checkoutPage.selectCreditCard("PA")
        await checkoutPage.enterCVV('900')
        await checkoutPage.clickSaveAndContinueButton()
        const totalShippingCost = await checkoutPage.shippingCost.innerText

        await checkoutPage.clickSubmitOrderButton()
        await loadingHelper.thisWait()

        const newOrderID = await orderDetailPage.getOrderID()


        // Vendor Side of Test
        await t.navigateTo(testConfig.adminAppUrl)
        await loginPage.login(testConfig.adminSellerUsername, testConfig.adminSellerPassword)
        await adminHeaderPage.selectSalesOrdersLink()
        await supplierOrdersPage.searchSelectOrder(newOrderID)
        await t.expect(supplierOrdersPage.shippingCost.withText(totalShippingCost).exists).ok()
    })