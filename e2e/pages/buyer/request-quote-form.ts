import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'

class RequestQuoteForm {
	submitForQuoteButton: Selector
	phoneField: Selector

	constructor() {
		this.submitForQuoteButton = Selector('button').withText(
			createRegExp('submit for quote')
		)
		this.phoneField = Selector('#Phone')
	}

	async clickSubmitForQuoteButton() {
		await t.click(this.submitForQuoteButton)
	}

	async enterPhoneNumber(number: string) {
		await t.typeText(this.phoneField, number)
	}
}

export default new RequestQuoteForm()
