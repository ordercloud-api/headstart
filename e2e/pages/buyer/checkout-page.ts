import { Selector, t } from 'testcafe'
import loadingHelper from '../../helpers/loading-helper'
import { createRegExp } from '../../helpers/regExp-helper'
import { TEST_CREDITCARD_NAME, TEST_CREDITCARD_NUMBER } from '../../test-constants'

class CheckoutPage {
	checkoutAsGuestButton: Selector
	saveAndContinueButton: Selector
	continueButton: Selector
	shippingForms: Selector
	creditCardDropdown: Selector
	creditCardOptions: Selector
	cvvField: Selector
	submitOrderButton: Selector
	shippingAddressDropdown: Selector
	shippingAddressOptions: Selector
	addNewAddressButton: Selector
	acceptButton: Selector
	//address form
	firstNameField: Selector
	lastNameField: Selector
	address1Field: Selector
	cityField: Selector
	stateDropdown: Selector
	stateOptions: Selector
	zipField: Selector
	phoneField: Selector
	//credit card form
	creditCardName: Selector
	creditCardNumber: Selector
	useShippingAddressButton: Selector

	shippingCost: Selector
	constructor() {
		this.checkoutAsGuestButton = Selector('button').withText(
			createRegExp('Checkout as Guest')
		)
		this.saveAndContinueButton = Selector('button').withText(
			createRegExp('save and continue')
		)
		this.continueButton = Selector('button').withText(
			createRegExp('continue')
		)
		this.shippingForms = Selector('ocm-lineitem-table')
		this.creditCardDropdown = Selector('#selectCard')
		this.creditCardOptions = this.creditCardDropdown.find('option')
		this.cvvField = Selector('#CVV')
		this.submitOrderButton = Selector('button').withText(
			createRegExp('submit order')
		)
		this.shippingAddressDropdown = Selector('#selectAddress')
		this.shippingAddressOptions = this.shippingAddressDropdown.find('option')
		this.addNewAddressButton = Selector('button').withText(
			createRegExp('add new address')
		)
		this.acceptButton = Selector('button').withText(createRegExp('accept'))
		//address form
		this.firstNameField = Selector('#FirstName')
		this.lastNameField = Selector('#LastName')
		this.address1Field = Selector('#Street1')
		this.cityField = Selector('#City')
		this.stateDropdown = Selector('#State')
		this.stateOptions = this.stateDropdown.find('option')
		this.zipField = Selector('#zipCode')
		this.phoneField = Selector('#Phone')
		//credit card form
		this.creditCardName = Selector('#name')
		this.creditCardNumber = Selector('#ccnumfield')
		this.useShippingAddressButton = Selector('#useShippingAddress')

		this.shippingCost = Selector('.font-italic.ng-star-inserted')
	}


	async clickCheckoutAsGuestButton() {
		await t.click(this.checkoutAsGuestButton)
	}

	async clickSaveAndContinueButton() {
		await t.click(this.saveAndContinueButton)
	}

	async clickContinueButton() {
		await t.click(this.continueButton)
	}

	async clickAcceptButton() {
		await t.click(this.acceptButton)
	}

	async selectShippingOption(product: string, shippingOption: string,) {
		const productForm = this.shippingForms.withText(createRegExp(product))
		const options = productForm.find('select')
		const selectedOption = options
			.find('option')
			.withText(createRegExp(shippingOption))
		await t.expect(options.visible).ok()
		await t.click(options)

		await t.click(selectedOption)

		//wait a little, the UI moves around when selecting shipping which is throwing TestCafe off
		await t.wait(2000)
	}

	async selectCreditCard(creditCardName: string) {
		await t.click(this.creditCardDropdown)
		await t.click(
			this.creditCardOptions.withText(createRegExp(creditCardName))
		)
	}

	async addCreditCardDuringCheckout() {
		await t.typeText(this.creditCardName, TEST_CREDITCARD_NAME)
		await t.wait(8000)
		await t.switchToIframe('#tokenFrame')
		await t.typeText(this.creditCardNumber, TEST_CREDITCARD_NUMBER, { paste: true })
		await t.switchToMainWindow()
		await t.typeText(this.address1Field, '110 N 5th St')
		await t.typeText(this.cityField, 'Minneapolis')
		await t.click(this.stateDropdown)
		await t.click(this.stateOptions.withText(createRegExp('MN')))
		await t.typeText(this.zipField, '55403')
	}


	async enterCVV(cvv: string) {
		await t.typeText(this.cvvField, cvv)
	}

	async clickSubmitOrderButton() {
		await t.click(this.submitOrderButton)
	}

	async clickAddNewAddressButton() {
		await t.click(this.addNewAddressButton)
	}

	async enterDefaultAddress(firstName: string, lastName: string) {
		await t.typeText(this.firstNameField, firstName)
		await t.typeText(this.lastNameField, lastName)
		await t.typeText(this.address1Field, '110 N 5th St')
		await t.typeText(this.cityField, 'Minneapolis')
		await t.click(this.stateDropdown)
		await t.click(this.stateOptions.withText(createRegExp('MN')))
		await t.typeText(this.zipField, '55403')
		await t.typeText(this.phoneField, '1231231234')
	}

	async shippingCostValue() {
		const shippingCostValue = this.shippingCost
		return shippingCostValue
	}
}

export default new CheckoutPage()
