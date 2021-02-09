import { Selector, t } from 'testcafe'
import faker = require('faker')
import { createRegExp } from '../../helpers/regExp-helper'
import randomString from '../../helpers/random-string'
import loadingHelper from '../../helpers/loading-helper'

class WarehouseDetailsPage {
	createButton: Selector
	addressNameField: Selector
	companyNameField: Selector
	street1Field: Selector
	cityField: Selector
	stateField: Selector
	zipField: Selector
	countryDropdown: Selector
	countryOptions: Selector

	constructor() {
		this.createButton = Selector('button')
			.withText(createRegExp('create'))
			.withAttribute('type', 'submit')
		this.addressNameField = Selector('#AddressName')
		this.companyNameField = Selector('input').withAttribute(
			'formcontrolname',
			'CompanyName'
		)
		this.street1Field = Selector('input').withAttribute(
			'formcontrolname',
			'Street1'
		)
		this.cityField = Selector('#City')
		this.stateField = Selector('#State')
		this.zipField = Selector('#Zip')
		this.countryDropdown = Selector('#Country')
		this.countryOptions = this.countryDropdown.find('option')
	}

	async createDefaultWarehouse() {
		const addressName = `AutomationAddress_${randomString(5)}`
		await t.typeText(this.addressNameField, addressName)
		await t.typeText(this.companyNameField, addressName)
		await t.click(this.countryDropdown)
		await t.click(
			this.countryOptions.withText(createRegExp('united states of america'))
		)
		await t.typeText(this.street1Field, '700 American Ave #200')
		await t.typeText(this.cityField, 'King Of Prussia')
		await t.typeText(this.stateField, 'PA')
		await t.typeText(this.zipField, '19406')
		await t.click(this.createButton)

		await loadingHelper.waitForTwoLoadingBars()

		return addressName
	}

	async createWarehouse(warehouseName: string, companyName: string) {
		await t.typeText(this.addressNameField, warehouseName)
		await t.typeText(this.companyNameField, companyName)
		await t.click(this.countryDropdown)
		await t.click(
			this.countryOptions.withText(createRegExp('united states of america'))
		)
		await t.typeText(this.street1Field, '700 American Ave #200')
		await t.typeText(this.cityField, 'King Of Prussia')
		await t.typeText(this.stateField, 'PA')
		await t.typeText(this.zipField, '19406')
		await t.click(this.createButton)

		await loadingHelper.waitForTwoLoadingBars()
	}
}

export default new WarehouseDetailsPage()
