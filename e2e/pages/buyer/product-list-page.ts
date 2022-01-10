import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'

class ProductListPage {
	products: Selector
	facets: Selector
	productSortBy: Selector
	sortByOptions: Selector
	productName: Selector

	constructor() {
		this.products = Selector('ocm-product-card')
		this.facets = Selector('ocm-facet-multiselect')
		this.productSortBy = Selector('ocm-product-sort')
		this.sortByOptions = this.productSortBy.find('option')
		this.productName = Selector('h5')
	}

	async clickProduct(product: string) {
		await t.click(this.products.withText(createRegExp(product)))
	}

	async applyFacet(facetName: string, facetSelection: string) {
		const selectedFacet = this.facets.withText(createRegExp(facetName))
		const facetlabels = selectedFacet.find('label')
		const facetSelectionValue = facetlabels.withText(
			createRegExp(facetSelection)
		)

		await t.click(facetSelectionValue)
	}

	async sortProducts(sortOption: string) {
		const sortByDropdown = this.productSortBy.find('select')
		await t.click(sortByDropdown)
		const selectedSortOption = this.sortByOptions.withText(
			createRegExp(sortOption)
		)
		await t.expect(selectedSortOption.exists).ok()
		await t.click(selectedSortOption)
	}
}

export default new ProductListPage()
