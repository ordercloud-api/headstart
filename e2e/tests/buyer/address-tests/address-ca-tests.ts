/* eslint-disable prettier/prettier */
import testConfig from '../../../testConfig'
import {
	adminClientSetup,
	buyerTestSetup,
	baseTestCleanup,
	existingBuyerTestSetup,
} from '../../../helpers/test-setup'
import buyerHeaderPage from '../../../pages/buyer/buyer-header-page'
import addressBookPage from '../../../pages/buyer/address-book-page'
import addressBookForm from '../../../pages/buyer/address-book-form'
import { createDefaultBuyer } from '../../../api-utils.ts/buyer-util'
import { createDefaultCatalog } from '../../../api-utils.ts/catalog-util'
import { createDefaultBuyerLocation, createDefaultCanadianBuyerLocation } from '../../../api-utils.ts/buyer-locations-util'

fixture`Address Tests (CA)`
	.meta('TestRun', 'HS')
	.before(async ctx => {
		ctx.adminClientAuth = await adminClientSetup()
	})
	.beforeEach(async t => {
		t.ctx.testUser = await existingBuyerTestSetup(`${testConfig.buyerUsername}11`, testConfig.BuyerPassword)
	})
	.page(testConfig.buyerAppUrl)

test('Can I add a Canadian address? | 20091', async t => {
	await buyerHeaderPage.clickAccountButton()
	await buyerHeaderPage.clickMyAddressesLink()
	await addressBookPage.clickAddAddressButton()
	await addressBookForm.enterDefaultCAAddress()
	await addressBookForm.clickSaveAddressButton()
	await t
		.expect(
			await addressBookPage.addedOrEditedAddressExists('1150 Lorne Park Rd')
		)
		.ok()
	await t
		.expect(
			await addressBookPage.addedOrEditedAddressExists(
				'Mississauga, ON L5H 3A7'
			)
		)
		.ok()
	await addressBookPage.clickDeleteAddressButton()
	await addressBookPage.clickConfirmDeleteAddressButton()
})

test('Can I edit a Canadian address? | 20092', async t => {
	await buyerHeaderPage.clickAccountButton()
	await buyerHeaderPage.clickMyAddressesLink()
	await addressBookPage.clickAddAddressButton()
	await addressBookForm.enterDefaultCAAddress()
	await addressBookForm.clickSaveAddressButton()
	await t
		.expect(
			await addressBookPage.addedOrEditedAddressExists('1150 Lorne Park Rd')
		)
		.ok()
	await t
		.expect(
			await addressBookPage.addedOrEditedAddressExists(
				'Mississauga, ON L5H 3A7'
			)
		)
		.ok()
	await addressBookPage.clickEditAddressButton()
	await addressBookForm.editFirstName('John')
	await addressBookForm.editLastName('Smith')
	await addressBookForm.editStreet1('1910 North Parallel Rd')
	await addressBookForm.editCity('Abbotsford')
	await addressBookForm.selectState('BC')
	await addressBookForm.editZip('V3G 2C6')
	await addressBookForm.editPhone('6045551234')
	await addressBookForm.clickSaveAddressButton()
	await t
		.expect(
			await addressBookPage.addedOrEditedAddressExists(
				'1910 North Parallel Rd'
			)
		)
		.ok()
	await t
		.expect(
			await addressBookPage.addedOrEditedAddressExists(
				'Abbotsford, BC V3G 2C6'
			)
		)
		.ok()
	await addressBookPage.clickDeleteAddressButton()
	await addressBookPage.clickConfirmDeleteAddressButton()
})
