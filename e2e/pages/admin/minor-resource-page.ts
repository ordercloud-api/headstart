import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'
import loadingHelper from '../../helpers/loading-helper'

class MinorResourcePage {
	createButton: Selector
	parentResourceDropdown: Selector
	parentResourceSearch: Selector
	resourceList: Selector

	constructor() {
		this.createButton = Selector('button').withText(createRegExp('create'))
		this.parentResourceDropdown = Selector('#parentresourcedropdown').nth(0)
		this.parentResourceSearch = Selector('#resource-search')
			.filterVisible()
			.nth(0)
		this.resourceList = Selector('table').find('tr.selectable-row')
	}

	async clickCreateButton() {
		await t.click(this.createButton)
		await loadingHelper.waitForLoadingBar()
	}

	async selectParentResourceDropdown(resource: string) {
		await t.click(this.parentResourceDropdown)
		await t.typeText(this.parentResourceSearch, resource)
		const supplierOption = this.parentResourceDropdown
			.parent()
			.find('.ps-content')
			.find('span')
			.withText(createRegExp(resource))
		await t.click(supplierOption)
		await loadingHelper.waitForLoadingBar()
	}

	async resourceExists(resource: string) {
		//wait for element to exist, then return if it exists
		//kinda silly, but want to do the assertion in the test, not here
		const resourceElement = this.resourceList.withText(resource)
		await t.wait(30)
		await t.expect(resourceElement.exists).ok()
		return await resourceElement.exists
	}

	async clickResource(resource: string) {
		await t.click(this.resourceList.withText(resource))
		await loadingHelper.waitForLoadingBar()
	}

	async selectResourceByIndex(index: number) {
		await t.click(this.resourceList.nth(index))
		await loadingHelper.waitForLoadingBar()
	}
}

export default new MinorResourcePage()
