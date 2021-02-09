import { Selector, t } from 'testcafe'
import loadingHelper from '../../helpers/loading-helper'
import { createRegExp } from '../../helpers/regExp-helper'

class ShoppingCartPage {
	checkoutButton: Selector
	products: Selector

	constructor() {
		this.checkoutButton = Selector('button').withText(
			createRegExp('checkout')
		)
		this.products = Selector('.position-relative')
	}

	async clickCheckoutButton() {
		await t.click(this.checkoutButton)
	}

	async removeProduct(productName: string) {
		const product = this.products.withText(createRegExp(productName))
		const removeButton = product.find('button')
		await t.expect(removeButton.exists).ok()
		await t.click(removeButton)
	}
}

export default new ShoppingCartPage()
