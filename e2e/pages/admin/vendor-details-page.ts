import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'
import randomString from '../../helpers/random-string'
import loadingHelper from '../../helpers/loading-helper'
import { scrollIntoView } from '../../helpers/element-helper'

class VendorDetailsPage {
	activeToggle: Selector
	nameField: Selector
	currencySelector: Selector
	currencyOptions: Selector
	standardProductTypeCheckbox: Selector
	quoteProductTypeCheckbox: Selector
	USCountryCheckbox: Selector
	CACountryCheckbox: Selector
	freightPOPToggle: Selector
	createButton: Selector

	constructor() {
		this.activeToggle = Selector('#Active').parent()
		this.nameField = Selector('#Name')
		this.currencySelector = Selector('#Currency')
		this.currencyOptions = this.currencySelector.find('option')
		this.standardProductTypeCheckbox = Selector('#Standard').parent()
		this.quoteProductTypeCheckbox = Selector('#Quote').parent()
		this.USCountryCheckbox = Selector('#US').parent()
		this.CACountryCheckbox = Selector('#CA').parent()
		this.freightPOPToggle = Selector('#SyncFreightPop').parent()
		this.createButton = Selector('button')
			.withText(createRegExp('create'))
			.withAttribute('type', 'submit')
	}

	async createDefaultVendor() {
		const vendorName = `AutomationVendor_${randomString(5)}`
		await t.click(this.activeToggle)
		await t.typeText(this.nameField, vendorName)
		await t.click(this.currencySelector)
		await t.click(
			this.currencyOptions.withText(createRegExp('united states dollar'))
		)
		await t.click(this.freightPOPToggle)
		await t.click(this.standardProductTypeCheckbox)
		// await t.click(this.USCountryCheckbox)
		await t.click(this.createButton)

		await loadingHelper.thisWait()

		return vendorName
	}

	//active, names, currency, product type, category
	async createVendor(
		active: boolean,
		name: string,
		currency: string,
		productType: string[],
		mainCategory: string,
		subCategory: string
	) {
		if (active) {
			await t.click(this.activeToggle)
		}
		await t.typeText(this.nameField, name)
		await t.click(this.currencySelector)
		await t.click(this.currencyOptions.withText(createRegExp(currency)))
		await t.click(this.freightPOPToggle)
		if (productType.includes('Standard')) {
			await t.click(this.standardProductTypeCheckbox)
		}
		if (productType.includes('Quote')) {
			await t.click(this.quoteProductTypeCheckbox)
		}

		await t.click(this.createButton)

		await loadingHelper.thisWait()
	}
}

export default new VendorDetailsPage()
