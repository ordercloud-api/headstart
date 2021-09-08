import { Selector, t } from 'testcafe'
import randomString from '../../helpers/random-string'
import { createRegExp } from '../../helpers/regExp-helper'
import {
	scrollIntoView,
	clickLeftOfElement,
} from '../../helpers/element-helper'
import loadingHelper from '../../helpers/loading-helper'

class ProductDetailsPage {
	nameField: Selector
	skuField: Selector
	activeToggle: Selector
	quantityPerUnitField: Selector
	unitOfMeasureField: Selector
	taxCategoryDropdown: Selector
	taxCategoryOptions: Selector
	taxCodeDropdown: Selector
	taxCodeOptions: Selector
	productWeightField: Selector
	shipAddressDropdown: Selector
	shipAddressOptions: Selector
	pricingTab: Selector
	buyerVisibilityTab: Selector
	priceField: Selector
	createButton: Selector
	sizeTierDropdown: Selector
	sizeTierOptions: Selector
	buyerList: Selector
	acceptChangesButton: Selector

	constructor() {
		this.nameField = Selector('#Name')
		this.skuField = Selector('#ID')
		this.activeToggle = Selector('label')
			.withText(createRegExp('active'))
			.parent()
			.find('span')
		this.quantityPerUnitField = Selector('#UnitOfMeasureQty')
		this.unitOfMeasureField = Selector('#UnitOfMeasureUnit')
		this.taxCategoryDropdown = Selector('#TaxCodeCategory')
		this.taxCategoryOptions = this.taxCategoryDropdown.find('option')
		this.productWeightField = Selector('#ShipWeight')
		this.taxCodeDropdown = Selector('#productTaxCodeDropdown')
		this.taxCodeOptions = this.taxCodeDropdown.parent().find('button')
		this.shipAddressDropdown = Selector('#ShipFromAddressID')
		this.shipAddressOptions = this.shipAddressDropdown.find('option')
		this.pricingTab = Selector('a').withText(createRegExp('pricing'))
		this.buyerVisibilityTab = Selector('a').withText(
			createRegExp('buyer visibility')
		)
		this.priceField = Selector('#Price')
		this.createButton = Selector('button')
			.withText(createRegExp('create'))
			.withAttribute('type', 'submit')
		this.acceptChangesButton = Selector('button').withText('Accept')
		this.sizeTierDropdown = Selector('#SizeTier')
		this.sizeTierOptions = this.sizeTierDropdown.find('option')
		this.buyerList = Selector('.list-group-item')
		this.acceptChangesButton = Selector('button').withText(
			createRegExp('Accept')
		)
	}

	async createDefaultStandardProduct() {
		const productName = `AutomationProduct_${randomString(5)}`
		await t.typeText(this.nameField, productName)
		await t.click(this.activeToggle)
		await t.typeText(this.quantityPerUnitField, '1')
		await t.typeText(this.unitOfMeasureField, 'Unit')
		await scrollIntoView('#TaxCodeCategory')
		await t.click(this.taxCategoryDropdown)
		await t.click(this.taxCategoryOptions.withText(createRegExp('freight')))
		await t.click(this.taxCodeDropdown)
		await t.click(
			this.taxCodeOptions.withText(
				createRegExp('delivery by company vehicle')
			)
		)
		await t.click(this.shipAddressDropdown)
		await t.click(
			this.shipAddressOptions.withText(createRegExp('automation'))
		)
		await t.typeText(this.productWeightField, '5')
		await t.click(this.sizeTierDropdown)
		await t.click(
			this.sizeTierOptions.withText(createRegExp('2 - 5 units will fit'))
		)
		await t.click(this.pricingTab)
		await t.typeText(this.priceField, '5')
		await clickLeftOfElement(this.priceField)
		await t.click(this.createButton)
		await loadingHelper.waitForLoadingBar()

		return productName
	}

	async createDefaultActiveStandardProduct() {
		const productName = `AutomationProduct_${randomString(5)}`
		await t.typeText(this.nameField, productName)
		await t.click(this.activeToggle)
		await scrollIntoView('.custom-control.custom-checkbox')
		await t.wait(3000)
		await t.typeText(this.quantityPerUnitField, '1')
		await t.typeText(this.unitOfMeasureField, 'Unit')
		await scrollIntoView('#TaxCodeCategory')
		await t.click(this.taxCategoryDropdown)
		await t.click(this.taxCategoryOptions.withText(createRegExp('freight')))
		await t.click(this.taxCodeDropdown)
		await t.click(
			this.taxCodeOptions.withText(
				createRegExp('delivery by company vehicle after passage of title')
			)
		)
		await t.click(this.shipAddressDropdown)
		await t.click(
			this.shipAddressOptions.withText(createRegExp('automation'))
		)
		await scrollIntoView('#SizeTier')
		await t.typeText(this.productWeightField, '5')
		await t.click(this.sizeTierDropdown)
		await t.click(
			this.sizeTierOptions.withText(createRegExp('2 - 5 units will fit'))
		)
		await t.click(this.pricingTab)
		await t.typeText(this.priceField, '5')
		await clickLeftOfElement(this.priceField)
		await t.click(this.createButton)
		await loadingHelper.waitForLoadingBar()

		return productName
	}

	async createDefaultQuoteProduct() {
		const productName = `AutomationProduct_${randomString(5)}`
		await t.typeText(this.nameField, productName)
		await t.typeText(this.quantityPerUnitField, '1')
		await t.typeText(this.unitOfMeasureField, 'Unit')
		await scrollIntoView(`button[type="submit"]`)
		await t.click(this.createButton)
		await loadingHelper.waitForLoadingBar()

		return productName
	}

	async createProduct(name: string, warehouse: string) {
		await t.typeText(this.nameField, name)
		await t.click(this.activeToggle)
		await t.typeText(this.quantityPerUnitField, '1')
		await t.typeText(this.unitOfMeasureField, 'Unit')
		await scrollIntoView('#TaxCodeCategory')
		await t.click(this.taxCategoryDropdown)
		await t.click(this.taxCategoryOptions.withText(createRegExp('freight')))
		await t.click(this.taxCodeDropdown)
		await t.click(
			this.taxCodeOptions.withText(
				createRegExp('delivery by company vehicle')
			)
		)
		await t.click(this.shipAddressDropdown)
		await t.click(this.shipAddressOptions.withText(createRegExp(warehouse)))
		await t.typeText(this.productWeightField, '5')
		await t.click(this.sizeTierDropdown)
		await t.click(
			this.sizeTierOptions.withText(createRegExp('2 - 5 units will fit'))
		)
		await t.click(this.pricingTab)
		await t.typeText(this.priceField, '5')
		await clickLeftOfElement(this.priceField)
		await t.click(this.createButton)
		await loadingHelper.waitForLoadingBar()
	}

	async clickBuyerVisibilityTab() {
		await t.click(this.buyerVisibilityTab)
		await loadingHelper.waitForLoadingBar()
	}

	async clickAcceptChangesButton() {
		await t.click(this.acceptChangesButton)
		await loadingHelper.waitForLoadingBar()
	}

	async getBuyerIndex(buyer: string) {
		// const element = this.buyerList.withText(createRegExp(buyer))
		// element.
		const elements = this.buyerList.addCustomDOMProperties({
			index: el => {
				const nodes = Array.prototype.slice.call(el.parentElement.children)
				return nodes.indexOf(el)
			},
		})

		//@ts-ignore
		return await elements.withText(createRegExp(buyer)).index
	}

	async editBuyerVisibility(buyerID: string) {
		const buyerIndex = await this.getBuyerIndex(buyerID)
		await scrollIntoView(`.list-group-item:nth-of-type(${buyerIndex})`)
		const thisBuyer = this.buyerList.withText(createRegExp(buyerID))
		const editButton = thisBuyer.find('button').withText(createRegExp('edit'))
		await t.click(editButton)
		await loadingHelper.waitForLoadingBar()
		await scrollIntoView(`button[type="submit"]`)
		const thisCatalog = Selector('tr')
			.withText(createRegExp('AutomationCatalog'))
			.find('label')
		await t.click(thisCatalog)
		await t.click(Selector('button').withText(createRegExp('save')))
		await loadingHelper.waitForLoadingBar()
	}
	async editBuyerVisibilityForView(buyerID: string, buyerCatalog: string) {
		const buyerIndex = await this.getBuyerIndex(buyerID)
		await scrollIntoView(`.list-group-item:nth-of-type(${buyerIndex})`)
		const thisBuyer = this.buyerList.withText(createRegExp(buyerID))
		const editButton = thisBuyer.find('button').withText(createRegExp('edit'))
		await t.click(editButton)
		await loadingHelper.waitForLoadingBar()

		const thisCatalog = Selector('tr')
			.withText(createRegExp(buyerCatalog))
			.find('label')
		await t.click(thisCatalog)
		await scrollIntoView(`button[type="submit"]`)
		await t.click(Selector('button').withText(createRegExp('save')))

		await loadingHelper.waitForLoadingBar()
	}
}

export default new ProductDetailsPage()
