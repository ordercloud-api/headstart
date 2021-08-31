import { Selector, t } from 'testcafe'
import loadingHelper from '../../helpers/loading-helper'
import { createRegExp } from '../../helpers/regExp-helper'

class ShoppingCartPage {
	checkoutButton: Selector
	products: Selector
	removeItemButton: Selector
	productsName: Selector

	constructor() {
		this.checkoutButton = Selector('button').withText(
			createRegExp('checkout')
		)
		this.products = Selector('.position-relative.border-bottom.border-light.px-0.lineitem.ng-star-inserted')
		this.removeItemButton = Selector('button').withText(createRegExp('Remove Item'))
		this.productsName = Selector('.text-dark.font-weight-medium.line-height-1')
	}

	async clickCheckoutButton() {
		await t.click(this.checkoutButton)
	}

	async removeProduct() {
		await t.click(this.removeItemButton)
	}
}

export default new ShoppingCartPage()
