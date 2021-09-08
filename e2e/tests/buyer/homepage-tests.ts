/* eslint-disable prettier/prettier */
import { ClientFunction } from 'testcafe'
import testConfig from '../../testConfig'
import {
    adminClientSetup,
    buyerTestSetup,
    baseTestCleanup,
    existingBuyerTestSetup,
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
import { createDefaultBuyer, deleteBuyer } from '../../api-utils.ts/buyer-util'
import { createDefaultCatalog, deleteCatalog, saveCatalogProductAssignment } from '../../api-utils.ts/catalog-util'
import { createDefaultBuyerLocation, deleteBuyerLocation } from '../../api-utils.ts/buyer-locations-util'
import { userClientAuth, supplierUserRoles } from '../../api-utils.ts/auth-util'
import { createDefaultProduct, deleteProduct, saveProductAssignment } from '../../api-utils.ts/product-util'
import { createDefaultSupplierAddress } from '../../api-utils.ts/warehouse-util'
import { delay } from '../../helpers/wait-helper'

const getLocation = ClientFunction(() => document.location.href)

fixture`Homepage Tests`
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
        ctx.catalogID = catalog.ID
        const location = await createDefaultBuyerLocation(
            ctx.buyerID,
            ctx.clientAuth,
        )
        ctx.locationID = location.Address.ID
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
    .beforeEach(async t => {
        t.ctx.testUser = await existingBuyerTestSetup(`${testConfig.buyerUsername}4`, testConfig.BuyerPassword)

    })
    .after(async ctx => {
        await deleteProduct(ctx.productID, ctx.supplierUserAuth)
        await deleteBuyerLocation(ctx.buyerID, ctx.locationID, ctx.clientAuth)
        await deleteCatalog(ctx.catalogID, ctx.buyerID, ctx.clientAuth)
        await deleteBuyer(ctx.buyerID, ctx.clientAuth)

    })
    .page(testConfig.buyerAppUrl)

test('Can I click brand hyperlink and be brought to homepage? | 2433', async t => {
    await buyerHeaderPage.clickAccountButton()
    await buyerHeaderPage.clickMyCreditCardsLink()
    await buyerHeaderPage.clickHomepageBrandLogo()
    // Assert that you're brought to home
    await t.expect(getLocation()).contains('home')

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
