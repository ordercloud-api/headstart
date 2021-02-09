import { Selector, t } from 'testcafe'
import loadingHelper from '../helpers/loading-helper'

class LoginPage {
	usernameInput: Selector
	passwordInput: Selector
	submitButton: Selector

	constructor() {
		this.usernameInput = Selector('#username')
		this.passwordInput = Selector('#password')
		this.submitButton = Selector('#submitBtn')
	}

	async login(username: string, password: string) {
		await t.typeText(this.usernameInput, username)
		await t.typeText(this.passwordInput, password)
		await t.click(this.submitButton)
		await loadingHelper.waitForLoadingBar()
	}
}

export default new LoginPage()
