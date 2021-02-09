import { Selector } from 'testcafe'

class SupplierListPage {
	supplierTitleH1: Selector

	constructor() {
		this.supplierTitleH1 = Selector('h1').withAttribute(
			'id',
			'supplier-title'
		)
	}
}

export default new SupplierListPage()
