import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'
import loadingHelper from '../../helpers/loading-helper'

class AdminHeaderPage {
	accountDropdown: Selector
	logoutButton: Selector
	vendorsDropdown: Selector
	productsDropdown: Selector
	allVendorsLink: Selector
	usersLink: Selector
	warehousesLink: Selector
	brandsDropdown: Selector
	allBrandsLink: Selector
	catalogsLink: Selector
	locationsLink: Selector
	allProductsLink: Selector
	promotionsLink: Selector
	ordersDropdown: Selector
	salesOrdersLink: Selector

	constructor() {
		this.accountDropdown = Selector('a.nav-link__user')
		this.logoutButton = Selector('a').withAttribute('href', '/login')
		this.vendorsDropdown = Selector('a').withText(createRegExp('vendor'))
		this.allVendorsLink = Selector('a').withText(createRegExp('all vendors'))
		this.usersLink = Selector('a').withText(createRegExp('users'))
		this.warehousesLink = Selector('a').withText(createRegExp('warehouses'))
		this.brandsDropdown = Selector('a').withText(createRegExp('brands'))
		this.allBrandsLink = Selector('a').withText(createRegExp('all brands'))
		this.catalogsLink = Selector('a').withText(createRegExp('catalogs'))
		this.locationsLink = Selector('a').withText(createRegExp('locations'))
		this.productsDropdown = Selector('a').withText(createRegExp('products'))
		this.allProductsLink = Selector('a').withText(
			createRegExp('all products')
		)
		this.promotionsLink = Selector('a').withText(createRegExp('promotions'))
		this.ordersDropdown = Selector('a').withText(createRegExp('orders'))
		this.salesOrdersLink = Selector('a').withText(
			createRegExp('sales orders')
		)
	}

	async logout() {
		await t.click(this.accountDropdown)
		await t.click(this.logoutButton)
	}

	async selectAllVendors() {
		await t.click(this.vendorsDropdown)
		await t.click(this.allVendorsLink)
	}

	async selectVendorUsers() {
		await t.click(this.vendorsDropdown)
		await t.click(this.usersLink.filterVisible().nth(0))
	}

	async selectVendorWarehouses() {
		await t.click(this.vendorsDropdown)
		await t.click(this.warehousesLink.filterVisible().nth(0))
	}

	async selectAllBrands() {
		await t.click(this.brandsDropdown)
		await t.click(this.allBrandsLink)
	}

	async selectBrandCatalogs() {
		await t.click(this.brandsDropdown)
		await t.click(this.catalogsLink.filterVisible().nth(0))
	}

	async selectBrandLocations() {
		await t.click(this.brandsDropdown)
		await t.click(this.locationsLink.filterVisible().nth(0))
		await loadingHelper.waitForLoadingBar()
	}

	async selectBrandUsers() {
		await t.click(this.brandsDropdown)
		await t.click(this.usersLink.filterVisible().nth(0))
		await loadingHelper.waitForLoadingBar()
	}

	async selectAllProducts() {
		await t.click(this.productsDropdown)
		await t.click(this.allProductsLink)
		await loadingHelper.waitForLoadingBar()
	}

	async selectPromotionsLink() {
		await t.click(this.productsDropdown)
		await t.click(this.promotionsLink)
		await loadingHelper.waitForLoadingBar()
	}

	async selectSalesOrdersLink() {
		await t.click(this.ordersDropdown)
		await t.click(this.salesOrdersLink)
		await loadingHelper.waitForLoadingBar()
	}
}

export default new AdminHeaderPage()
