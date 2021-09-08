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
import { createDefaultBuyerLocation } from '../../../api-utils.ts/buyer-locations-util'

fixture`Address Tests (US)`
	.meta('TestRun', 'HS')
	.before(async ctx => {
		ctx.adminClientAuth = await adminClientSetup()
	})
	.page(testConfig.buyerAppUrl)

test.before(async t => {
	t.ctx.testUser = await existingBuyerTestSetup(`${testConfig.buyerUsername}`, testConfig.BuyerPassword)
})('Can I add a valid American address? | 20086', async t => {
	await buyerHeaderPage.clickAccountButton()
	await buyerHeaderPage.clickMyAddressesLink()
	await addressBookPage.clickAddAddressButton()
	await addressBookForm.enterDefaultUSAddress()
	await addressBookForm.clickSaveAddressButton()
	await t
		.expect(
			await addressBookPage.addedOrEditedAddressExists(
				'110 N 5th St Ste 300'
			)
		)
		.ok()
	await t
		.expect(
			await addressBookPage.addedOrEditedAddressExists(
				'Minneapolis, MN 55403-1631'
			)
		)
		.ok()
	await addressBookPage.clickDeleteAddressButton()
	await addressBookPage.clickConfirmDeleteAddressButton()
})

test.before(async t => {
	t.ctx.testUser = await existingBuyerTestSetup(`${testConfig.buyerUsername}16`, testConfig.BuyerPassword)
})('Does SmartyStreets offer suggestions for invalid addresses? | 20087', async t => {
	await buyerHeaderPage.clickAccountButton()
	await buyerHeaderPage.clickMyAddressesLink()
	await addressBookPage.clickAddAddressButton()
	await addressBookForm.enterFirstName('Jane')
	await addressBookForm.enterLastName('Doe')
	await addressBookForm.enterStreet1('110 N 5th St')
	await addressBookForm.enterStreet2('Suite 300')
	await addressBookForm.enterCity('King of Prussia')
	await addressBookForm.selectState('PA')
	await addressBookForm.enterZip('55444')
	await addressBookForm.enterPhone('6515554545')
	await addressBookForm.clickSaveAddressButton()
	await addressBookForm.selectAndSaveAddressSuggestion()
	await addressBookPage.clickDeleteAddressButton()
	await addressBookPage.clickConfirmDeleteAddressButton()
})

test.before(async t => {
	t.ctx.testUser = await existingBuyerTestSetup(`${testConfig.buyerUsername}13`, testConfig.BuyerPassword)
})('Are required fields being enforced? | 20088', async t => {
	await buyerHeaderPage.clickAccountButton()
	await buyerHeaderPage.clickMyAddressesLink()
	await addressBookPage.clickAddAddressButton()
	await addressBookForm.enterFirstName('Jane')
	await addressBookForm.enterLastName('Doe')
	await addressBookForm.enterStreet1('110 N 5th St')
	await addressBookForm.enterCity('Minneapolis')
	await addressBookForm.enterZip('55403')
	// No state
	await addressBookForm.isButtonDisabled()
	await addressBookForm.selectState('MN')
	await addressBookForm.removeFirstName()
	// No first name
	await addressBookForm.isButtonDisabled()
	await addressBookForm.enterFirstName('Jane')
	await addressBookForm.removeLastName()
	// No last name
	await addressBookForm.isButtonDisabled()
	await addressBookForm.enterLastName('Doe')
	await addressBookForm.removeStreet1()
	// No street 1
	await addressBookForm.isButtonDisabled()
	await addressBookForm.enterStreet1('110 N 5th St')
	await addressBookForm.removeZip()
	// No zip
	await addressBookForm.isButtonDisabled()
	await addressBookForm.enterZip('55403')
	// Now okay to save
	await addressBookForm.clickSaveAddressButton()
	await t
		.expect(await addressBookPage.addedOrEditedAddressExists('110 N 5th St'))
		.ok()
	await addressBookPage.clickDeleteAddressButton()
	await addressBookPage.clickConfirmDeleteAddressButton()
})

test.before(async t => {
	t.ctx.testUser = await existingBuyerTestSetup(`${testConfig.buyerUsername}14`, testConfig.BuyerPassword)
})('Can I delete an address? | 20089', async t => {
	await buyerHeaderPage.clickAccountButton()
	await buyerHeaderPage.clickMyAddressesLink()
	await addressBookPage.clickAddAddressButton()
	await addressBookForm.enterDefaultUSAddress()
	await addressBookForm.clickSaveAddressButton()
	await t
		.expect(
			await addressBookPage.addedOrEditedAddressExists(
				'110 N 5th St Ste 300'
			)
		)
		.ok()
	await t
		.expect(
			await addressBookPage.addedOrEditedAddressExists(
				'Minneapolis, MN 55403-1631'
			)
		)
		.ok()
	await addressBookPage.clickDeleteAddressButton()
	await addressBookPage.clickConfirmDeleteAddressButton()
})

test.before(async t => {
	t.ctx.testUser = await existingBuyerTestSetup(`${testConfig.buyerUsername}15`, testConfig.BuyerPassword)
})('Can I edit an American address? | 20090', async t => {
	await buyerHeaderPage.clickAccountButton()
	await buyerHeaderPage.clickMyAddressesLink()
	await addressBookPage.clickAddAddressButton()
	await addressBookForm.enterDefaultUSAddress()
	await addressBookForm.clickSaveAddressButton()
	await t
		.expect(
			await addressBookPage.addedOrEditedAddressExists(
				'110 N 5th St Ste 300'
			)
		)
		.ok()
	await t
		.expect(
			await addressBookPage.addedOrEditedAddressExists(
				'Minneapolis, MN 55403-1631'
			)
		)
		.ok()
	await addressBookPage.clickEditAddressButton()
	await addressBookForm.editFirstName('John')
	await addressBookForm.editLastName('Smith')
	await addressBookForm.editStreet1('9910 Wadsworth Pkwy')
	await addressBookForm.editStreet2('Suite 100')
	await addressBookForm.editCity('Westminster')
	await addressBookForm.selectState('CO')
	await addressBookForm.editZip('80021')
	await addressBookForm.editPhone('7205559876')
	await addressBookForm.clickSaveAddressButton()
	await t
		.expect(
			await addressBookPage.addedOrEditedAddressExists(
				'9910 Wadsworth Pkwy Unit 100'
			)
		)
		.ok()
	await t
		.expect(
			await addressBookPage.addedOrEditedAddressExists(
				'Westminster, CO 80021-4295'
			)
		)
		.ok()
	await addressBookPage.clickDeleteAddressButton()
	await addressBookPage.clickConfirmDeleteAddressButton()
})
