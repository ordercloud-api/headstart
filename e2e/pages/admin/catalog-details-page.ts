import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'
import randomString from '../../helpers/random-string'
import loadingHelper from '../../helpers/loading-helper'

class CatalogDetailsPage {
	nameField: Selector
	createButton: Selector

	constructor() {
		this.nameField = Selector('#Name')
		this.createButton = Selector('button')
			.withText(createRegExp('create'))
			.withAttribute('type', 'submit')
	}

	async createDefaultCatalog() {
		const catalogName = `AutomationCatalog_${randomString(5)}`
		await t.typeText(this.nameField, catalogName)
		await t.click(this.createButton)

		await loadingHelper.waitForLoadingBar()

		return catalogName
	}
}

export default new CatalogDetailsPage()
