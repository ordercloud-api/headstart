/* eslint-disable prettier/prettier */
import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'

class AddressBookForm {
	firstNameField: Selector
	lastNameField: Selector
	street1Field: Selector
	street2Field: Selector
	cityField: Selector
	stateDropdown: Selector
	stateOptions: Selector
	zipField: Selector
	phoneField: Selector
	saveAddressButton: Selector
	selectAddressSuggestion: Selector

	constructor() {
		this.firstNameField = Selector('#FirstName').nth(1)
		this.lastNameField = Selector('#LastName').nth(1)
		this.street1Field = Selector('#Street1').nth(1)
		this.street2Field = Selector('#Street2').nth(1)
		this.cityField = Selector('#City').nth(1)
		this.stateDropdown = Selector('#State').nth(1)
		this.stateOptions = this.stateDropdown.find('option')
		this.zipField = Selector('#zipCode').nth(1)
		this.phoneField = Selector('#Phone').nth(1)
		this.saveAddressButton = Selector('#address-save-button').nth(1)
		this.selectAddressSuggestion = Selector('.svg-inline--fa.fa-square.fa-w-14').nth(1)
	}

	// Default addresses whenever adding
	async enterDefaultUSAddress() {
		await this.enterFirstName('Jane')
		await this.enterLastName('Doe')
		await this.enterStreet1('110 N 5th St')
		await this.enterStreet2('Suite 300')
		await this.enterCity('Minneapolis')
		await this.selectState('MN')
		await this.enterZip('55403')
		await this.enterPhone('6515554545')
	}

	async enterDefaultCAAddress() {
		await this.enterFirstName('Jane')
		await this.enterLastName('Doe')
		await this.enterStreet1('1150 Lorne Park Rd')
		await this.enterCity('Mississauga')
		await this.selectState('ON')
		await this.enterZip('L5H 3A7')
		await this.enterPhone('2265554545')
	}

	// First Name
	async enterFirstName(firstName: string) {
		await t.typeText(this.firstNameField, firstName)
	}

	async editFirstName(firstName: string) {
		await t.typeText(this.firstNameField, firstName, { replace: true })
	}

	async removeFirstName() {
		await t.click(this.firstNameField)
		await t.pressKey('ctrl+a delete')
	}

	// Last Name
	async enterLastName(lastName: string) {
		await t.typeText(this.lastNameField, lastName)
	}

	async editLastName(lastName: string) {
		await t.typeText(this.lastNameField, lastName, { replace: true })
	}

	async removeLastName() {
		await t.click(this.lastNameField)
		await t.pressKey('ctrl+a delete')
	}

	// Street 1
	async enterStreet1(street1: string) {
		await t.typeText(this.street1Field, street1)
	}

	async editStreet1(street1: string) {
		await t.typeText(this.street1Field, street1, { replace: true })
	}

	async removeStreet1() {
		await t.click(this.street1Field)
		await t.pressKey('ctrl+a delete')
	}

	// Street 2
	async enterStreet2(street2: string) {
		await t.typeText(this.street2Field, street2)
	}

	async editStreet2(street2: string) {
		await t.typeText(this.street2Field, street2, { replace: true })
	}

	// City
	async enterCity(city: string) {
		await t.typeText(this.cityField, city)
	}

	async editCity(city: string) {
		await t.typeText(this.cityField, city, { replace: true })
	}

	// State
	async selectState(state: string) {
		await t.click(this.stateDropdown)
		await t.click(this.stateOptions.withText(createRegExp(state)))
	}

	// Zip
	async enterZip(zip: string) {
		await t.typeText(this.zipField, zip)
	}

	async editZip(zip: string) {
		await t.typeText(this.zipField, zip, { replace: true })
	}

	async removeZip() {
		await t.click(this.zipField)
		await t.pressKey('ctrl+a delete')
	}

	// Phone
	async enterPhone(phone: string) {
		await t.typeText(this.phoneField, phone)
	}

	async editPhone(phone: string) {
		await t.typeText(this.phoneField, phone, { replace: true })
	}

	// Save
	async clickSaveAddressButton() {
		await t.click(this.saveAddressButton)
	}

	async isButtonDisabled() {
		await t.expect(this.saveAddressButton.hasAttribute('disabled')).ok()
	}

	async selectAndSaveAddressSuggestion() {
		await t.click(this.selectAddressSuggestion)
		await t.click(this.saveAddressButton)
	}
}

export default new AddressBookForm()
