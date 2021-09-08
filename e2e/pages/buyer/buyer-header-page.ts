import { Selector, t } from 'testcafe'
import loadingHelper from '../../helpers/loading-helper'
import { createRegExp } from '../../helpers/regExp-helper'

class BuyerHeaderPage {
	homepageBrandLogo: Selector
	accountDropdown: Selector
	logoutButton: Selector
	productsLink: Selector
	suppliersLink: Selector
	searchBar: Selector
	cartButton: Selector
	myProfileLink: Selector
	myAddressesLink: Selector
	myLocationsLink: Selector
	myCreditCardsLink: Selector
	ordersDropdown: Selector
	placedByMeLink: Selector
	placedInMyLocationsLink: Selector
	awaitingMyApprovalLink: Selector

	constructor() {
		this.accountDropdown = Selector('#accountDropdown').withText(
			createRegExp('account')
		)
		this.homepageBrandLogo = Selector('.navbar-brand.clickable.cursor-pointer.ng-tns-c247-0')
		this.logoutButton = Selector('a').withText('Logout')
		this.productsLink = Selector('a').withText(createRegExp('products'))
		this.suppliersLink = Selector('#suppliers-link')
		this.searchBar = Selector('#search-addon')
		this.cartButton = Selector('span').withText(createRegExp('cart'))
		this.myProfileLink = Selector('a').withText(createRegExp('my profile'))
		this.myAddressesLink = Selector('a').withText(
			createRegExp('my addresses')
		)
		this.myLocationsLink = Selector('a').withText(
			createRegExp('my locations')
		)
		this.myCreditCardsLink = Selector('a').withText(
			createRegExp('my credit cards')
		)
		this.ordersDropdown = Selector('#account-dropdown').withText(
			createRegExp('orders')
		)
		this.placedByMeLink = Selector('a').withText(createRegExp('placed by me'))
		this.placedInMyLocationsLink = Selector('a').withText(
			createRegExp('placed in my locations')
		)
		this.awaitingMyApprovalLink = Selector('a').withText(
			createRegExp('awaiting my approval')
		)
	}

	async clickAccountButton() {
		await t.click(this.accountDropdown)
	}

	async clickOrdersButton() {
		await t.click(this.ordersDropdown)
	}

	async clickMyProfileLink() {
		await t.click(this.myProfileLink)
	}

	async clickMyAddressesLink() {
		await t.click(this.myAddressesLink)
	}

	async clickMyLocationsLink() {
		await t.click(this.myLocationsLink)
	}

	async clickMyCreditCardsLink() {
		await t.click(this.myCreditCardsLink)
	}

	async clickPlacedByMeLink() {
		await t.click(this.placedByMeLink)
	}

	async clickPlacedInMyLocationsLink() {
		await t.click(this.placedInMyLocationsLink)
	}

	async clickAwaitingMyApprovalLink() {
		await t.click(this.awaitingMyApprovalLink)
	}

	async clickHomepageBrandLogo() {
		await t.click(this.homepageBrandLogo)
	}

	async logout() {
		await this.clickAccountButton()
		await t.click(this.logoutButton)
	}

	async clickProductsLink() {
		await t.click(this.productsLink)
	}

	async clickSuppliersLink() {
		await t.click(this.suppliersLink)
	}

	async search(searchText: string) {
		await t.typeText(this.searchBar, searchText)
		await loadingHelper.waitForLoadingBar()
	}

	async clearSearchText() {
		await t.click(this.searchBar).pressKey('ctrl+a delete')
	}

	async clickCartButton() {
		await t.click(this.cartButton)
	}
}

export default new BuyerHeaderPage()
