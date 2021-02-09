/* eslint-disable prettier/prettier */
import { Selector, t } from 'testcafe'
import { createRegExp } from '../../helpers/regExp-helper'

class AddressBookPage {
	addAddressButton: Selector
	address: Selector
	smartyStreetsSuggestionHeader: Selector
	confirmAddressDeleteButton: Selector

	constructor() {
		this.addAddressButton = Selector('#add-address-button')
		this.address = Selector('ocm-address-card')
		this.smartyStreetsSuggestionHeader = Selector('address-suggestion')
		this.confirmAddressDeleteButton = Selector('#confirm-address-delete')
	}

	async clickAddAddressButton() {
		await t.click(this.addAddressButton)
	}

	async clickDeleteAddressButton() {
		const slottedAddressDeleteButton = Selector(() =>
			document
				.querySelectorAll('ocm-address-card')[1]
				.querySelector('.address-delete')
		)
		await t.click(slottedAddressDeleteButton)
	}

	async clickConfirmDeleteAddressButton() {
		await t.click(this.confirmAddressDeleteButton)
	}

	async clickEditAddressButton() {
		const slottedAddressEditButton = Selector(() =>
			document
				.querySelectorAll('ocm-address-card')[1]
				.querySelector('.address-edit')
		)
		await t.click(slottedAddressEditButton)
	}

	async addedOrEditedAddressExists(address: string) {
		await this.address()
		const shadowAddressDataExists = Selector(() =>
			document
				.querySelectorAll('ocm-address-card')[1]
				.shadowRoot.querySelector('.address-detail')
		).withText(createRegExp(address)).exists
		return shadowAddressDataExists
	}

	async suggestedAddressExists() {
		const shadowAddressDataExists = Selector(() =>
			document
				.querySelectorAll('ocm-address-card')[1]
				.shadowRoot.querySelector('.address-detail')
		).exists
		return shadowAddressDataExists
	}

	async deletedAddressExists() {
		return Selector(() => document.querySelectorAll('ocm-address-card')[1])
			.exists
	}
}

export default new AddressBookPage()
