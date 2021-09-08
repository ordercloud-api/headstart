/* eslint-disable prettier/prettier */
import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'

class Homepage {
	featuredProductsH3: Selector
	loginButton: Selector
	registerButton: Selector

	constructor() {
		this.featuredProductsH3 = Selector('h3').withText(createRegExp('featured products'))
		this.loginButton = Selector('a').withText('Login')
		this.registerButton = Selector('a').withText('Register')
	}
	async selectLoginbutton() {
		await t.click(this.loginButton)
	}

}

export default new Homepage()
