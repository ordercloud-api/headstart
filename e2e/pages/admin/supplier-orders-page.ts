import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'
import randomString from '../../helpers/random-string'
import loadingHelper from '../../helpers/loading-helper'
import { scrollIntoView } from '../../helpers/element-helper'

class SupplierOrdersPage {
	ordersList: Selector
	orderDropdownList: Selector
	searchBarField: Selector
	createShipmentButton: Selector
	trackingNumberField: Selector
	carrierField: Selector
	shippingServiceField: Selector
	shipAllItemsCheckbox: Selector
	submitOrderButton: Selector
	progressBar: Selector
	shippingCost: Selector

	constructor() {
		this.ordersList = Selector('.selectable-row')
		this.orderDropdownList = Selector('.dropdown-item')
		this.searchBarField = Selector('#product-search')
		this.createShipmentButton = Selector('button').withText(
			createRegExp('Create Shipment')
		)
		this.trackingNumberField = Selector('#trackingNumber')
		this.carrierField = Selector('#Shipper')
		this.shippingServiceField = Selector('#Service')
		this.shipAllItemsCheckbox = Selector('.custom-control-label').withText(
			'Ship All Items'
		)
		this.submitOrderButton = Selector('#submitBtn')
		this.progressBar = Selector('div').withAttribute('role', 'progressbar')
		this.shippingCost = Selector('.col-md-5').nth(1)
	}

	async selectOrdersList() {
		await t.click(this.ordersList)
		await t.click(this.orderDropdownList)
	}

	async getProgressBarStatus() {
		return await this.progressBar.textContent
	}

	async searchSelectOrder(orderID: string) {
		await t.typeText(this.searchBarField, orderID)
		await t.click(this.ordersList.withText(orderID))
	}

	async createShipment(orderID: string) {
		await t.typeText(this.searchBarField, orderID)
		await t.click(this.ordersList.withText(orderID))
		await t.click(this.createShipmentButton)
		await t.typeText(this.trackingNumberField, 'testTracking Number 123')
		await t.typeText(this.carrierField, 'fedex')
		await t.typeText(this.shippingServiceField, 'fedex-2')
		await scrollIntoView(`button[type="submit"]`)
		await t.click(this.shipAllItemsCheckbox)
		await t.click(this.submitOrderButton)
		await loadingHelper.waitForLoadingBar()
	}
}

export default new SupplierOrdersPage()
