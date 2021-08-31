import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'
import loadingHelper from '../../helpers/loading-helper'

class AdminHeaderPage {
	accountDropdown: Selector
	logoutButton: Selector
	suppliersDropdown: Selector
	productsDropdown: Selector
	allSuppliersLink: Selector
	usersLink: Selector
	warehousesLink: Selector
	buyersDropdown: Selector
	allbuyersLink: Selector
	catalogsLink: Selector
	locationsLink: Selector
	allProductsLink: Selector
	promotionsLink: Selector
	ordersDropdown: Selector
	ordersDropdownSpecificLink: Selector
	reportsDropdown: Selector
	processReports: Selector
	reportsTemplates: Selector

	constructor() {
		this.accountDropdown = Selector('a.nav-link__user')
		this.logoutButton = Selector('a').withAttribute('href', '/login')
		this.suppliersDropdown = Selector('a').withText(createRegExp('supplier'))
		this.allSuppliersLink = Selector('a').withText(createRegExp('all suppliers'))
		this.usersLink = Selector('a').withText(createRegExp('users'))
		this.warehousesLink = Selector('a').withText(createRegExp('Supplier Addresses'))
		this.buyersDropdown = Selector('a').withText(createRegExp('buyers'))
		this.allbuyersLink = Selector('a').withText(createRegExp('all buyers'))
		this.catalogsLink = Selector('a').withText(createRegExp('catalogs'))
		this.locationsLink = Selector('a').withText(createRegExp('Buyer Groups'))
		this.productsDropdown = Selector('a').withText(createRegExp('products'))
		this.allProductsLink = Selector('a').withText(
			createRegExp('all products')
		)
		this.promotionsLink = Selector('a').withText(createRegExp('promotions'))
		this.ordersDropdown = Selector('a').withText(createRegExp('orders'))
		this.ordersDropdownSpecificLink = Selector('.dropdown-item').withText(
			'Orders'
		)
		this.reportsDropdown = Selector('a').withText(createRegExp('Reports'))
		this.processReports = Selector('a').withText(createRegExp('Process Reports'))
		this.reportsTemplates = Selector('a').withText(createRegExp('Report Templates'))

	}

	async logout() {
		await t.click(this.accountDropdown)
		await t.click(this.logoutButton)
	}

	async selectAllSuppliers() {
		await t.click(this.suppliersDropdown)
		await t.click(this.allSuppliersLink)
	}

	async selectSupplierUsers() {
		await t.click(this.suppliersDropdown)
		await t.click(this.usersLink.filterVisible().nth(0))
	}

	async selectSupplierWarehouses() {
		await t.click(this.suppliersDropdown)
		await t.click(this.warehousesLink.filterVisible().nth(0))
	}

	async selectAllbuyers() {
		await t.click(this.buyersDropdown)
		await t.click(this.allbuyersLink)
	}

	async selectbuyerCatalogs() {
		await t.click(this.buyersDropdown)
		await t.click(this.catalogsLink.filterVisible().nth(0))
	}

	async selectbuyerLocations() {
		await t.click(this.buyersDropdown)
		await t.click(this.locationsLink.filterVisible().nth(0))
		await loadingHelper.waitForLoadingBar()
	}

	async selectbuyerUsers() {
		await t.click(this.buyersDropdown)
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
		await t.click(this.ordersDropdownSpecificLink)
		await loadingHelper.waitForLoadingBar()
	}

	async selectProcessReports() {
		await t.click(this.reportsDropdown)
		await t.click(this.processReports)
	}

	async selectReportsTemplates() {
		await t.click(this.reportsDropdown)
		await t.click(this.reportsTemplates)
	}
}

export default new AdminHeaderPage()
