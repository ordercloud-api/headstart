/* eslint-disable prettier/prettier */
import { ClientFunction } from 'testcafe'
import testConfig from '../../testConfig'
import {
    adminClientSetup,
    buyerTestSetup,
    baseTestCleanup,
} from '../../helpers/test-setup'
import buyerHeaderPage from '../../pages/buyer/buyer-header-page'
import '../../helpers/loading-helper'
import homepage from '../../pages/buyer/homepage'
import productListPage from '../../pages/buyer/product-list-page'
import supplierListPage from '../../pages/buyer/supplier-list-page'
import myProfilePage from '../../pages/buyer/my-profile-page'
import myAddressesPage from '../../pages/buyer/my-addresses-page'
import myLocationsPage from '../../pages/buyer/my-locations-page'
import myCreditCardsPage from '../../pages/buyer/my-credit-cards-page'

const getLocation = ClientFunction(() => document.location.href)

fixture`Homepage Tests`
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

test('Can I click brand hyperlink and be brought to homepage? | 2433', async t => {
    await buyerHeaderPage.clickAccountButton()
    await buyerHeaderPage.clickMyCreditCardsLink()
    await buyerHeaderPage.clickHomepageBrandLogo()
    // Assert that you're brought to home
    await t.expect(getLocation()).contains('home')
    await t.expect(homepage.featuredProductsH3.exists).ok()
})

test('Can I navigate to products list page? | 2434', async t => {
    await buyerHeaderPage.clickProductsLink()
    // Assert that you're brought to product list page
    await t.expect(getLocation()).contains('products')
    await t.expect(productListPage.products.exists).ok()
})

test('Can I navigate to suppliers list page? | 2435', async t => {
    await buyerHeaderPage.clickSuppliersLink()
    // Assert that you're brought to product list page
    await t.expect(getLocation()).contains('suppliers')
    await t.expect(supplierListPage.supplierTitleH1.exists).ok()
})

test('Can I click the account dropdown and have all of the options populate? | 2466', async t => {
    await buyerHeaderPage.clickAccountButton()
    // Assert that all the dropdown options are populated
    await t.expect(buyerHeaderPage.myProfileLink.exists).ok()
    await t.expect(buyerHeaderPage.myAddressesLink.exists).ok()
    await t.expect(buyerHeaderPage.myLocationsLink.exists).ok()
    await t.expect(buyerHeaderPage.myCreditCardsLink.exists).ok()
    await t.expect(buyerHeaderPage.logoutButton.exists).ok()
})

test('Can I click the my profile link and be brought to the my profile page? | 20072', async t => {
    await buyerHeaderPage.clickAccountButton()
    await buyerHeaderPage.clickMyProfileLink()
    // Assert that you are brought to the my profile page
    await t.expect(myProfilePage.accountDetailsH1.exists).ok()
    await buyerHeaderPage.clickAccountButton()
    await t.expect(buyerHeaderPage.myProfileLink.hasClass('active')).ok()
})

test('Can I click the my addresses link and be brought to the my addresses page? | 20073', async t => {
    await buyerHeaderPage.clickAccountButton()
    await buyerHeaderPage.clickMyAddressesLink()
    // Assert that you are brought to the my addresses page
    await t.expect(myAddressesPage.addressBookH1.exists).ok()
    await buyerHeaderPage.clickAccountButton()
    await t.expect(buyerHeaderPage.myAddressesLink.hasClass('active')).ok()
})

test('Can I click the my locations link and be brought to the my locations page? | 20074', async t => {
    await buyerHeaderPage.clickAccountButton()
    await buyerHeaderPage.clickMyLocationsLink()
    // Assert that you are brought to the my locations page
    await t.expect(myLocationsPage.locationsH1.exists).ok()
    await buyerHeaderPage.clickAccountButton()
    await t.expect(buyerHeaderPage.myLocationsLink.hasClass('active')).ok()
})

test('Can I click the my credit cards link and be brought to the my credit cards page? | 20075', async t => {
    await buyerHeaderPage.clickAccountButton()
    await buyerHeaderPage.clickMyCreditCardsLink()
    // Assert that you are brought to the my credit cards page
    await t.expect(myCreditCardsPage.paymentMethodsH1.exists).ok()
    await buyerHeaderPage.clickAccountButton()
    await t.expect(buyerHeaderPage.myCreditCardsLink.hasClass('active')).ok()
})
