import { Selector, t } from 'testcafe'
import loadingHelper from '../../helpers/loading-helper'
import { createRegExp } from '../../helpers/regExp-helper'

class OrderDetailPage {
	products: Selector

	constructor() {
		this.products = Selector('.card')
	}

	async productExists(product: string) {
		return this.products.withText(createRegExp(product)).exists
	}
}

export default new OrderDetailPage()
