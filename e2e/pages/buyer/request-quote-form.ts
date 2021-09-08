import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'

class RequestQuoteForm {
	submitForQuoteButton: Selector
	phoneField: Selector
	firstName: Selector
	lastName: Selector
	phoneNumber: Selector
	email: Selector

	constructor() {
		this.submitForQuoteButton = Selector('button').withText(
			createRegExp('submit for quote')
		)
		this.phoneField = Selector('#Phone')
		this.firstName = Selector('#FirstName')
		this.lastName = Selector('#LastName')
		this.phoneNumber = Selector('#Phone')
		this.email = Selector('#Email')
	}

	async clickSubmitForQuoteButton() {
		await t.typeText(this.phoneNumber, '1231231234')
		await t.click(this.submitForQuoteButton)
	}

	async enterPhoneNumber(number: string) {
		await t.typeText(this.phoneField, number)
	}
	async quoteSubmitForm() {
		const firstName = 'Boyle'
		const lastName = 'Palno'
		const email = `${firstName}${lastName}.hpmqx9la@mailosaur.io`
		await t.typeText(this.firstName, firstName)
		await t.typeText(this.lastName, lastName)
		await t.typeText(this.email, email)
		await t.typeText(this.phoneNumber, '1231231234')
	}
}

export default new RequestQuoteForm()
