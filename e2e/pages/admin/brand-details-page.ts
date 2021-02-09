import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'
import randomString from '../../helpers/random-string'
import loadingHelper from '../../helpers/loading-helper'

class BrandDetailsPage {
	nameField: Selector
	createButton: Selector

	constructor() {
		this.nameField = Selector('#Name')
		this.createButton = Selector('button.brand-button').withText(
			createRegExp('create')
		)
	}

	async createDefaultBrand() {
		const brandName = `AutomationBrand_${randomString(5)}`
		await t.typeText(this.nameField, brandName)
		await t.click(this.createButton)

		await loadingHelper.waitForLoadingBar()

		return brandName
	}
}

export default new BrandDetailsPage()
