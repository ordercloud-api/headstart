import { Selector, t } from 'testcafe'
import loadingHelper from '../../helpers/loading-helper'
import { createRegExp } from '../../helpers/regExp-helper'

class OrderDetailPage {
	products: Selector
	orderID: Selector

	constructor() {
		this.products = Selector('.card')
		this.orderID = Selector('a').withText(createRegExp('HDS_DEMO'))
	}

	async getOrderID() {
		return await this.orderID.innerText
	}

	async productExists(product: string) {
		return this.products.withText(createRegExp(product)).exists
	}
}

export default new OrderDetailPage()
