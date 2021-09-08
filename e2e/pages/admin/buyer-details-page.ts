import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'
import randomString from '../../helpers/random-string'
import loadingHelper from '../../helpers/loading-helper'

class buyerDetailsPage {
	nameField: Selector
	createButton: Selector

	constructor() {
		this.nameField = Selector('#Name')
		this.createButton = Selector('.btn.btn-primary').withExactText('Create')

	}

	async createDefaultbuyer() {
		const buyerName = `Automationbuyer_${randomString(5)}`
		await t.typeText(this.nameField, buyerName)
		await t.click(this.createButton)

		await loadingHelper.waitForLoadingBar()

		return buyerName
	}
}

export default new buyerDetailsPage()
